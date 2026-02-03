using HaircutHistoryApp.Api.Middleware;
using HaircutHistoryApp.Api.Services;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace HaircutHistoryApp.Api.Functions;

/// <summary>
/// Functions for profile management.
/// </summary>
public class ProfileFunctions
{
    private readonly ILogger<ProfileFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly FreeTierSettings _freeTierSettings;

    public ProfileFunctions(
        ILogger<ProfileFunctions> logger,
        ICosmosDbService cosmosDbService,
        IOptions<FreeTierSettings> freeTierSettings)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _freeTierSettings = freeTierSettings.Value;
    }

    /// <summary>
    /// GET /api/profiles - List all profiles owned by current user
    /// </summary>
    [Function("GetProfiles")]
    public async Task<HttpResponseData> GetProfilesAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<List<Profile>>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var profiles = await _cosmosDbService.GetProfilesByOwnerAsync(userId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<List<Profile>>.Ok(profiles));
        return response;
    }

    /// <summary>
    /// GET /api/profiles/{id} - Get a specific profile
    /// </summary>
    [Function("GetProfile")]
    public async Task<HttpResponseData> GetProfileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles/{id}")] HttpRequestData req,
        FunctionContext context,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Try to get profile as owner first
        var profile = await _cosmosDbService.GetProfileAsync(id, userId);

        // If not found as owner, check if it's shared with this user
        if (profile == null)
        {
            // Check if profile is shared with this user
            var share = await _cosmosDbService.GetShareAsync(id, userId);
            if (share != null)
            {
                // Get the profile using a cross-partition query
                profile = await GetProfileByIdCrossPartitionAsync(id);
            }
        }

        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<Profile>.Ok(profile));
        return response;
    }

    /// <summary>
    /// POST /api/profiles - Create a new profile
    /// </summary>
    [Function("CreateProfile")]
    public async Task<HttpResponseData> CreateProfileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profiles")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Check free tier limit
        var user = await _cosmosDbService.GetUserAsync(userId);
        if (user != null && !user.IsPremiumActive)
        {
            var profileCount = await _cosmosDbService.GetProfileCountAsync(userId);
            if (profileCount >= _freeTierSettings.MaxProfiles)
            {
                var limitResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await limitResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(
                    ErrorCodes.LimitExceeded,
                    $"Free tier allows {_freeTierSettings.MaxProfiles} profile(s). Upgrade to premium for unlimited profiles."));
                return limitResponse;
            }
        }

        var requestBody = await req.ReadFromJsonAsync<CreateProfileRequest>();
        if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.Name))
        {
            var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await validationResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.ValidationError, "Profile name is required"));
            return validationResponse;
        }

        var profile = new Profile
        {
            Id = Guid.NewGuid().ToString(),
            OwnerUserId = userId,
            Name = requestBody.Name,
            Description = requestBody.Description,
            Measurements = requestBody.Measurements ?? new List<Measurement>(),
            AvatarUrl = requestBody.AvatarUrl
        };

        profile = await _cosmosDbService.CreateProfileAsync(profile);
        _logger.LogInformation("Created profile {ProfileId} for user {UserId}", profile.Id, userId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(ApiResponse<Profile>.Ok(profile));
        return response;
    }

    /// <summary>
    /// PUT /api/profiles/{id} - Update a profile
    /// </summary>
    [Function("UpdateProfile")]
    public async Task<HttpResponseData> UpdateProfileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "profiles/{id}")] HttpRequestData req,
        FunctionContext context,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Only owner can update
        var profile = await _cosmosDbService.GetProfileAsync(id, userId);
        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<Profile>.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        var requestBody = await req.ReadFromJsonAsync<UpdateProfileRequest>();
        if (requestBody != null)
        {
            if (!string.IsNullOrWhiteSpace(requestBody.Name))
                profile.Name = requestBody.Name;
            if (requestBody.Description != null)
                profile.Description = requestBody.Description;
            if (requestBody.Measurements != null)
                profile.Measurements = requestBody.Measurements;
            if (requestBody.AvatarUrl != null)
                profile.AvatarUrl = requestBody.AvatarUrl;
        }

        profile = await _cosmosDbService.UpdateProfileAsync(profile);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<Profile>.Ok(profile));
        return response;
    }

    /// <summary>
    /// DELETE /api/profiles/{id} - Soft delete a profile
    /// </summary>
    [Function("DeleteProfile")]
    public async Task<HttpResponseData> DeleteProfileAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "profiles/{id}")] HttpRequestData req,
        FunctionContext context,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Only owner can delete
        var profile = await _cosmosDbService.GetProfileAsync(id, userId);
        if (profile == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.NotFound, "Profile not found"));
            return notFoundResponse;
        }

        await _cosmosDbService.DeleteProfileAsync(id, userId);
        _logger.LogInformation("Deleted profile {ProfileId} for user {UserId}", id, userId);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GET /api/profiles/shared - Get profiles shared with current user (stylist view)
    /// </summary>
    [Function("GetSharedProfiles")]
    public async Task<HttpResponseData> GetSharedProfilesAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles/shared")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<List<Profile>>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var profiles = await _cosmosDbService.GetSharedProfilesAsync(userId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<List<Profile>>.Ok(profiles));
        return response;
    }

    private async Task<Profile?> GetProfileByIdCrossPartitionAsync(string profileId)
    {
        // Cross-partition query to find profile by ID (used for shared profiles)
        return await _cosmosDbService.GetProfileByIdAsync(profileId);
    }
}
