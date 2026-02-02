using HaircutHistoryApp.Api.Middleware;
using HaircutHistoryApp.Api.Services;
using HaircutHistoryApp.Shared.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HaircutHistoryApp.Api.Functions;

/// <summary>
/// Functions for photo management.
/// </summary>
public class PhotoFunctions
{
    private readonly ILogger<PhotoFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IBlobService _blobService;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    public PhotoFunctions(
        ILogger<PhotoFunctions> logger,
        ICosmosDbService cosmosDbService,
        IBlobService blobService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _blobService = blobService;
    }

    /// <summary>
    /// POST /api/photos/upload - Get a SAS URL for uploading a photo
    /// </summary>
    [Function("GetPhotoUploadUrl")]
    public async Task<HttpResponseData> GetPhotoUploadUrlAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "photos/upload")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<PhotoUploadResponse>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Check if user is premium
        var user = await _cosmosDbService.GetUserAsync(userId);
        if (user == null || !user.IsPremiumActive)
        {
            var premiumResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await premiumResponse.WriteAsJsonAsync(ApiResponse<PhotoUploadResponse>.Fail(
                ErrorCodes.PremiumRequired,
                "Photo uploads require premium subscription."));
            return premiumResponse;
        }

        var requestBody = await req.ReadFromJsonAsync<PhotoUploadRequest>();
        if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.FileName) || string.IsNullOrWhiteSpace(requestBody.ContentType))
        {
            var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await validationResponse.WriteAsJsonAsync(ApiResponse<PhotoUploadResponse>.Fail(
                ErrorCodes.ValidationError,
                "fileName and contentType are required"));
            return validationResponse;
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(requestBody.ContentType))
        {
            var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await validationResponse.WriteAsJsonAsync(ApiResponse<PhotoUploadResponse>.Fail(
                ErrorCodes.ValidationError,
                "Only JPEG, PNG, and WebP images are allowed"));
            return validationResponse;
        }

        var (uploadUrl, blobUrl, expiresAt) = await _blobService.GetUploadUrlAsync(requestBody.FileName, requestBody.ContentType);

        var uploadResponse = new PhotoUploadResponse
        {
            UploadUrl = uploadUrl,
            BlobUrl = blobUrl,
            ExpiresAt = expiresAt
        };

        _logger.LogInformation("Generated photo upload URL for user {UserId}", userId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<PhotoUploadResponse>.Ok(uploadResponse));
        return response;
    }

    /// <summary>
    /// DELETE /api/photos/{blobName} - Delete a photo
    /// </summary>
    [Function("DeletePhoto")]
    public async Task<HttpResponseData> DeletePhotoAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "photos/{*blobUrl}")] HttpRequestData req,
        FunctionContext context,
        string blobUrl)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Decode the blob URL
        blobUrl = Uri.UnescapeDataString(blobUrl);

        // In a production app, you'd verify the user owns a haircut record that references this photo
        // For now, we'll just delete if they're authenticated and premium
        var user = await _cosmosDbService.GetUserAsync(userId);
        if (user == null || !user.IsPremiumActive)
        {
            var premiumResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await premiumResponse.WriteAsJsonAsync(ApiResponse.Fail(
                ErrorCodes.PremiumRequired,
                "Photo management requires premium subscription."));
            return premiumResponse;
        }

        await _blobService.DeleteBlobAsync(blobUrl);
        _logger.LogInformation("Deleted photo for user {UserId}: {BlobUrl}", userId, blobUrl);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
