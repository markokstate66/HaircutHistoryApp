using HaircutHistoryApp.Api.Middleware;
using HaircutHistoryApp.Api.Services;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HaircutHistoryApp.Api.Functions;

/// <summary>
/// Functions for user management.
/// </summary>
public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public UserFunctions(ILogger<UserFunctions> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    /// <summary>
    /// GET /api/users/me - Get current user
    /// </summary>
    [Function("GetCurrentUser")]
    public async Task<HttpResponseData> GetCurrentUserAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/me")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<User>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var user = await _cosmosDbService.GetUserAsync(userId);
        if (user == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<User>.Fail(ErrorCodes.NotFound, "User not found"));
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<User>.Ok(user));
        return response;
    }

    /// <summary>
    /// POST /api/users/me - Create or update current user
    /// </summary>
    [Function("CreateOrUpdateUser")]
    public async Task<HttpResponseData> CreateOrUpdateUserAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/me")] HttpRequestData req,
        FunctionContext context)
    {
        var userId = context.GetUserId();
        var userEmail = context.GetUserEmail();
        var userName = context.GetUserName();

        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await errorResponse.WriteAsJsonAsync(ApiResponse<User>.Fail(ErrorCodes.Unauthorized, "Authentication required"));
            return errorResponse;
        }

        var requestBody = await req.ReadFromJsonAsync<CreateOrUpdateUserRequest>();
        var displayName = requestBody?.DisplayName ?? userName ?? "User";

        var existingUser = await _cosmosDbService.GetUserAsync(userId);
        User user;

        if (existingUser == null)
        {
            // Create new user
            user = new User
            {
                Id = userId,
                Email = userEmail ?? "",
                DisplayName = displayName,
                IsPremium = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _logger.LogInformation("Creating new user: {UserId}", userId);
        }
        else
        {
            // Update existing user
            user = existingUser;
            user.DisplayName = displayName;
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Updating existing user: {UserId}", userId);
        }

        user = await _cosmosDbService.UpsertUserAsync(user);

        var response = req.CreateResponse(existingUser == null ? HttpStatusCode.Created : HttpStatusCode.OK);
        await response.WriteAsJsonAsync(ApiResponse<User>.Ok(user));
        return response;
    }
}
