using HaircutHistoryApp.Api.Middleware;
using HaircutHistoryApp.Api.Services;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HaircutHistoryApp.Api.Functions;

/// <summary>
/// Functions for profile sharing.
/// </summary>
public class ShareFunctions
{
    private readonly ILogger<ShareFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    // Simple token key - in production, use Azure Key Vault
    private static readonly byte[] TokenKey = Encoding.UTF8.GetBytes("HaircutHistory_ShareToken_Key_32!");

    public ShareFunctions(ILogger<ShareFunctions> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    /// <summary>
    /// POST /api/profiles/{profileId}/share - Generate a share link
    /// </summary>
    [Function("GenerateShareLink")]
    public async Task<HttpResponseData> GenerateShareLinkAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profiles/{profileId}/share")] HttpRequestData req,
        FunctionContext context,
        string profileId)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<ShareToken>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Only owner can generate share links
        var profile = await _cosmosDbService.GetProfileAsync(profileId, userId);
        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<ShareToken>.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        // Generate token (valid for 24 hours)
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var tokenData = new
        {
            profileId,
            ownerUserId = userId,
            expiresAt = expiresAt.Ticks
        };

        var tokenJson = JsonSerializer.Serialize(tokenData);
        var token = EncryptToken(tokenJson);

        var shareToken = new ShareToken
        {
            Token = token,
            ShareUrl = $"haircuthistory://share/{token}",
            ExpiresAt = expiresAt
        };

        _logger.LogInformation("Generated share link for profile {ProfileId}", profileId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<ShareToken>.Ok(shareToken));
        return response;
    }

    /// <summary>
    /// POST /api/share/accept/{token} - Accept a share invitation
    /// </summary>
    [Function("AcceptShare")]
    public async Task<HttpResponseData> AcceptShareAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "share/accept/{token}")] HttpRequestData req,
        FunctionContext context,
        string token)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Decrypt and validate token
        string? tokenJson;
        try
        {
            tokenJson = DecryptToken(token);
        }
        catch
        {
            var invalidResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidResponse.WriteAsJsonAsync(ApiResponse<object>.Fail(ErrorCodes.ValidationError, "Invalid share token"));
            return invalidResponse;
        }

        var tokenData = JsonSerializer.Deserialize<ShareTokenData>(tokenJson);
        if (tokenData == null)
        {
            var invalidResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalidResponse.WriteAsJsonAsync(ApiResponse<object>.Fail(ErrorCodes.ValidationError, "Invalid share token"));
            return invalidResponse;
        }

        // Check expiration
        var expiresAt = new DateTime(tokenData.ExpiresAt, DateTimeKind.Utc);
        if (DateTime.UtcNow > expiresAt)
        {
            var expiredResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await expiredResponse.WriteAsJsonAsync(ApiResponse<object>.Fail(
                ErrorCodes.TokenExpired,
                "This share link has expired. Ask the profile owner for a new link."));
            return expiredResponse;
        }

        // Don't allow sharing with yourself
        if (tokenData.OwnerUserId == userId)
        {
            var selfShareResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await selfShareResponse.WriteAsJsonAsync(ApiResponse<object>.Fail(ErrorCodes.ValidationError, "Cannot share a profile with yourself"));
            return selfShareResponse;
        }

        // Check if already shared
        var existingShare = await _cosmosDbService.GetShareAsync(tokenData.ProfileId, userId);
        if (existingShare != null)
        {
            var alreadySharedResponse = req.CreateResponse(HttpStatusCode.OK);
            await alreadySharedResponse.WriteAsJsonAsync(ApiResponse<object>.Ok(new
            {
                profileId = tokenData.ProfileId,
                message = "Profile already shared with you"
            }));
            return alreadySharedResponse;
        }

        // Create the share
        var share = new ProfileShare
        {
            Id = Guid.NewGuid().ToString(),
            ProfileId = tokenData.ProfileId,
            StylistUserId = userId,
            IsActive = true
        };

        await _cosmosDbService.CreateShareAsync(share);
        _logger.LogInformation("User {UserId} accepted share for profile {ProfileId}", userId, tokenData.ProfileId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<object>.Ok(new
        {
            profileId = tokenData.ProfileId,
            sharedAt = share.SharedAt
        }));
        return response;
    }

    /// <summary>
    /// DELETE /api/profiles/{profileId}/share/{stylistUserId} - Revoke a share
    /// </summary>
    [Function("RevokeShare")]
    public async Task<HttpResponseData> RevokeShareAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "profiles/{profileId}/share/{stylistUserId}")] HttpRequestData req,
        FunctionContext context,
        string profileId,
        string stylistUserId)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Only owner can revoke
        var profile = await _cosmosDbService.GetProfileAsync(profileId, userId);
        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        await _cosmosDbService.RevokeShareAsync(profileId, stylistUserId);
        _logger.LogInformation("Revoked share for profile {ProfileId} from stylist {StylistId}", profileId, stylistUserId);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GET /api/profiles/{profileId}/shares - List all shares for a profile
    /// </summary>
    [Function("GetShares")]
    public async Task<HttpResponseData> GetSharesAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles/{profileId}/shares")] HttpRequestData req,
        FunctionContext context,
        string profileId)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<List<ProfileShare>>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Only owner can view shares
        var profile = await _cosmosDbService.GetProfileAsync(profileId, userId);
        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<List<ProfileShare>>.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        var shares = await _cosmosDbService.GetSharesByProfileAsync(profileId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<List<ProfileShare>>.Ok(shares));
        return response;
    }

    #region Token Encryption

    private static string EncryptToken(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = TokenKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV and cipher text
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string DecryptToken(string cipherText)
    {
        // Restore base64 padding
        cipherText = cipherText.Replace('-', '+').Replace('_', '/');
        switch (cipherText.Length % 4)
        {
            case 2: cipherText += "=="; break;
            case 3: cipherText += "="; break;
        }

        var cipherBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = TokenKey;

        // Extract IV
        var iv = new byte[16];
        Buffer.BlockCopy(cipherBytes, 0, iv, 0, 16);
        aes.IV = iv;

        // Extract cipher text
        var cipher = new byte[cipherBytes.Length - 16];
        Buffer.BlockCopy(cipherBytes, 16, cipher, 0, cipher.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    #endregion

    private class ShareTokenData
    {
        public string ProfileId { get; set; } = "";
        public string OwnerUserId { get; set; } = "";
        public long ExpiresAt { get; set; }
    }
}
