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
/// Functions for haircut record management.
/// </summary>
public class HaircutRecordFunctions
{
    private readonly ILogger<HaircutRecordFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly FreeTierSettings _freeTierSettings;

    public HaircutRecordFunctions(
        ILogger<HaircutRecordFunctions> logger,
        ICosmosDbService cosmosDbService,
        IOptions<FreeTierSettings> freeTierSettings)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _freeTierSettings = freeTierSettings.Value;
    }

    /// <summary>
    /// GET /api/profiles/{profileId}/haircuts - List haircuts for a profile
    /// </summary>
    [Function("GetHaircuts")]
    public async Task<HttpResponseData> GetHaircutsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles/{profileId}/haircuts")] HttpRequestData req,
        FunctionContext context,
        string profileId)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<List<HaircutRecord>>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Check access
        if (!await _cosmosDbService.HasAccessToProfileAsync(profileId, userId))
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteAsJsonAsync(ApiResponse<List<HaircutRecord>>.Fail(ErrorCodes.Forbidden, "Access denied"));
            return forbiddenResponse;
        }

        // Parse query parameters
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var limit = int.TryParse(query["limit"], out var l) ? Math.Min(l, 100) : 50;
        var offset = int.TryParse(query["offset"], out var o) ? o : 0;

        var records = await _cosmosDbService.GetHaircutRecordsAsync(profileId, limit, offset);
        var total = await _cosmosDbService.GetHaircutRecordCountAsync(profileId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new PaginatedResponse<HaircutRecord>
        {
            Data = records,
            Total = total,
            Limit = limit,
            Offset = offset,
            Success = true
        });
        return response;
    }

    /// <summary>
    /// GET /api/profiles/{profileId}/haircuts/{id} - Get a specific haircut
    /// </summary>
    [Function("GetHaircut")]
    public async Task<HttpResponseData> GetHaircutAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles/{profileId}/haircuts/{id}")] HttpRequestData req,
        FunctionContext context,
        string profileId,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Check access
        if (!await _cosmosDbService.HasAccessToProfileAsync(profileId, userId))
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Forbidden, "Access denied"));
            return forbiddenResponse;
        }

        var record = await _cosmosDbService.GetHaircutRecordAsync(id, profileId);
        if (record == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.NotFound, "Haircut record not found"));
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Ok(record));
        return response;
    }

    /// <summary>
    /// POST /api/profiles/{profileId}/haircuts - Create a new haircut record
    /// </summary>
    [Function("CreateHaircut")]
    public async Task<HttpResponseData> CreateHaircutAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profiles/{profileId}/haircuts")] HttpRequestData req,
        FunctionContext context,
        string profileId)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        // Check access - owner or stylist with share access
        if (!await _cosmosDbService.HasAccessToProfileAsync(profileId, userId))
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Forbidden, "Access denied"));
            return forbiddenResponse;
        }

        // Check free tier limit (only for profile owner)
        var user = await _cosmosDbService.GetUserAsync(userId);
        if (user != null && !user.IsPremiumActive)
        {
            var recordCount = await _cosmosDbService.GetHaircutRecordCountAsync(profileId);
            if (recordCount >= _freeTierSettings.MaxHaircutsPerProfile)
            {
                var limitResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await limitResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(
                    ErrorCodes.LimitExceeded,
                    $"Free tier allows {_freeTierSettings.MaxHaircutsPerProfile} haircuts per profile. Upgrade to premium for unlimited."));
                return limitResponse;
            }
        }

        var requestBody = await req.ReadFromJsonAsync<CreateHaircutRecordRequest>();
        if (requestBody == null)
        {
            var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await validationResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.ValidationError, "Request body is required"));
            return validationResponse;
        }

        // Check if photos are allowed (premium only)
        if (requestBody.PhotoUrls.Count > 0 && (user == null || !user.IsPremiumActive))
        {
            var premiumResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await premiumResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(
                ErrorCodes.PremiumRequired,
                "Photos require premium subscription."));
            return premiumResponse;
        }

        var record = new HaircutRecord
        {
            Id = Guid.NewGuid().ToString(),
            ProfileId = profileId,
            CreatedByUserId = userId,
            Date = requestBody.Date,
            StylistName = requestBody.StylistName,
            Location = requestBody.Location,
            PhotoUrls = requestBody.PhotoUrls,
            Notes = requestBody.Notes,
            Price = requestBody.Price,
            DurationMinutes = requestBody.DurationMinutes
        };

        record = await _cosmosDbService.CreateHaircutRecordAsync(record);
        _logger.LogInformation("Created haircut record {RecordId} for profile {ProfileId}", record.Id, profileId);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Ok(record));
        return response;
    }

    /// <summary>
    /// PUT /api/profiles/{profileId}/haircuts/{id} - Update a haircut record
    /// </summary>
    [Function("UpdateHaircut")]
    public async Task<HttpResponseData> UpdateHaircutAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "profiles/{profileId}/haircuts/{id}")] HttpRequestData req,
        FunctionContext context,
        string profileId,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var record = await _cosmosDbService.GetHaircutRecordAsync(id, profileId);
        if (record == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.NotFound, "Haircut record not found"));
            return notFoundResponse;
        }

        // Only the creator can update
        if (record.CreatedByUserId != userId)
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Fail(ErrorCodes.Forbidden, "Only the creator can update this record"));
            return forbiddenResponse;
        }

        var requestBody = await req.ReadFromJsonAsync<UpdateHaircutRecordRequest>();
        if (requestBody != null)
        {
            if (requestBody.Date.HasValue) record.Date = requestBody.Date.Value;
            if (requestBody.StylistName != null) record.StylistName = requestBody.StylistName;
            if (requestBody.Location != null) record.Location = requestBody.Location;
            if (requestBody.PhotoUrls != null) record.PhotoUrls = requestBody.PhotoUrls;
            if (requestBody.Notes != null) record.Notes = requestBody.Notes;
            if (requestBody.Price.HasValue) record.Price = requestBody.Price;
            if (requestBody.DurationMinutes.HasValue) record.DurationMinutes = requestBody.DurationMinutes;
        }

        record = await _cosmosDbService.UpdateHaircutRecordAsync(record);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<HaircutRecord>.Ok(record));
        return response;
    }

    /// <summary>
    /// DELETE /api/profiles/{profileId}/haircuts/{id} - Delete a haircut record
    /// </summary>
    [Function("DeleteHaircut")]
    public async Task<HttpResponseData> DeleteHaircutAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "profiles/{profileId}/haircuts/{id}")] HttpRequestData req,
        FunctionContext context,
        string profileId,
        string id)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var record = await _cosmosDbService.GetHaircutRecordAsync(id, profileId);
        if (record == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.NotFound, "Haircut record not found"));
            return notFoundResponse;
        }

        // Profile owner can delete any record (even stylist-created ones)
        // The profile's owner needs to be checked
        if (!await _cosmosDbService.HasAccessToProfileAsync(profileId, userId))
        {
            var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbiddenResponse.WriteAsJsonAsync(ApiResponse.Fail(ErrorCodes.Forbidden, "Access denied"));
            return forbiddenResponse;
        }

        await _cosmosDbService.DeleteHaircutRecordAsync(id, profileId);
        _logger.LogInformation("Deleted haircut record {RecordId} from profile {ProfileId}", id, profileId);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
