using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HaircutHistoryApp.Shared.DTOs;
using HaircutHistoryApp.Shared.Models;

namespace HaircutHistoryApp.Services;

/// <summary>
/// HTTP client implementation for Azure Functions API.
/// </summary>
public class AzureApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogService _logService;
    private string? _accessToken;

    /// <summary>
    /// Event fired when a 401 Unauthorized response is received, indicating token refresh is needed.
    /// </summary>
    public event Func<Task<string?>>? OnTokenRefreshNeeded;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AzureApiService(HttpClient httpClient, ILogService logService)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(FirebaseConfig.EffectiveApiBaseUrl + "/");
        _logService = logService;
    }

    public void SetAccessToken(string? token)
    {
        _accessToken = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// Attempts to refresh the token if a 401 is received.
    /// </summary>
    private async Task<bool> TryRefreshTokenAsync()
    {
        if (OnTokenRefreshNeeded == null)
            return false;

        try
        {
            var newToken = await OnTokenRefreshNeeded.Invoke();
            if (!string.IsNullOrEmpty(newToken))
            {
                SetAccessToken(newToken);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", "Token refresh failed", ex);
        }

        return false;
    }

    #region User Operations

    public async Task<ApiResponse<User>> GetCurrentUserAsync()
    {
        return await GetAsync<User>("users/me");
    }

    public async Task<ApiResponse<User>> CreateOrUpdateUserAsync(string displayName)
    {
        var request = new CreateOrUpdateUserRequest { DisplayName = displayName };
        return await PostAsync<User>("users/me", request);
    }

    #endregion

    #region Profile Operations

    public async Task<ApiResponse<List<Profile>>> GetProfilesAsync()
    {
        return await GetAsync<List<Profile>>("profiles");
    }

    public async Task<ApiResponse<Profile>> GetProfileAsync(string profileId)
    {
        return await GetAsync<Profile>($"profiles/{profileId}");
    }

    public async Task<ApiResponse<Profile>> CreateProfileAsync(string name, string? avatarUrl = null)
    {
        var request = new CreateProfileRequest { Name = name, AvatarUrl = avatarUrl };
        return await PostAsync<Profile>("profiles", request);
    }

    public async Task<ApiResponse<Profile>> UpdateProfileAsync(string profileId, string? name = null, string? avatarUrl = null)
    {
        var request = new UpdateProfileRequest { Name = name, AvatarUrl = avatarUrl };
        return await PutAsync<Profile>($"profiles/{profileId}", request);
    }

    public async Task<ApiResponse> DeleteProfileAsync(string profileId)
    {
        return await DeleteAsync($"profiles/{profileId}");
    }

    public async Task<ApiResponse<List<Profile>>> GetSharedProfilesAsync()
    {
        return await GetAsync<List<Profile>>("profiles/shared");
    }

    #endregion

    #region Haircut Record Operations

    public async Task<PaginatedResponse<HaircutRecord>> GetHaircutRecordsAsync(string profileId, int limit = 50, int offset = 0)
    {
        return await GetHaircutRecordsInternalAsync(profileId, limit, offset, isRetry: false);
    }

    private async Task<PaginatedResponse<HaircutRecord>> GetHaircutRecordsInternalAsync(string profileId, int limit, int offset, bool isRetry)
    {
        try
        {
            var endpoint = $"profiles/{profileId}/haircuts?limit={limit}&offset={offset}";
            var response = await _httpClient.GetAsync(endpoint);

            // Handle 401 - try to refresh token and retry once
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
            {
                _logService.Warning("AzureApiService", $"GET {endpoint} returned 401, attempting token refresh");
                if (await TryRefreshTokenAsync())
                {
                    return await GetHaircutRecordsInternalAsync(profileId, limit, offset, isRetry: true);
                }
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<PaginatedResponse<HaircutRecord>>(content, JsonOptions)
                    ?? new PaginatedResponse<HaircutRecord>();
            }

            _logService.Error("AzureApiService", $"GetHaircutRecords failed: {content}");
            return new PaginatedResponse<HaircutRecord> { Success = false };
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", "GetHaircutRecords exception", ex);
            return new PaginatedResponse<HaircutRecord> { Success = false };
        }
    }

    public async Task<ApiResponse<HaircutRecord>> GetHaircutRecordAsync(string profileId, string recordId)
    {
        return await GetAsync<HaircutRecord>($"profiles/{profileId}/haircuts/{recordId}");
    }

    public async Task<ApiResponse<HaircutRecord>> CreateHaircutRecordAsync(string profileId, CreateHaircutRecordRequest request)
    {
        return await PostAsync<HaircutRecord>($"profiles/{profileId}/haircuts", request);
    }

    public async Task<ApiResponse<HaircutRecord>> UpdateHaircutRecordAsync(string profileId, string recordId, UpdateHaircutRecordRequest request)
    {
        return await PutAsync<HaircutRecord>($"profiles/{profileId}/haircuts/{recordId}", request);
    }

    public async Task<ApiResponse> DeleteHaircutRecordAsync(string profileId, string recordId)
    {
        return await DeleteAsync($"profiles/{profileId}/haircuts/{recordId}");
    }

    #endregion

    #region Share Operations

    public async Task<ApiResponse<ShareToken>> GenerateShareLinkAsync(string profileId)
    {
        return await PostAsync<ShareToken>($"profiles/{profileId}/share", null);
    }

    public async Task<ApiResponse<object>> AcceptShareAsync(string token)
    {
        return await PostAsync<object>($"share/accept/{token}", null);
    }

    public async Task<ApiResponse> RevokeShareAsync(string profileId, string stylistUserId)
    {
        return await DeleteAsync($"profiles/{profileId}/share/{stylistUserId}");
    }

    public async Task<ApiResponse<List<ProfileShare>>> GetSharesAsync(string profileId)
    {
        return await GetAsync<List<ProfileShare>>($"profiles/{profileId}/shares");
    }

    #endregion

    #region Photo Operations

    public async Task<ApiResponse<PhotoUploadResponse>> GetPhotoUploadUrlAsync(string fileName, string contentType)
    {
        var request = new PhotoUploadRequest { FileName = fileName, ContentType = contentType };
        return await PostAsync<PhotoUploadResponse>("photos/upload", request);
    }

    public async Task<ApiResponse> DeletePhotoAsync(string blobUrl)
    {
        return await DeleteAsync($"photos/{Uri.EscapeDataString(blobUrl)}");
    }

    #endregion

    #region HTTP Helpers

    private async Task<ApiResponse<T>> GetAsync<T>(string endpoint, bool isRetry = false)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            // Handle 401 - try to refresh token and retry once
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
            {
                _logService.Warning("AzureApiService", $"GET {endpoint} returned 401, attempting token refresh");
                if (await TryRefreshTokenAsync())
                {
                    return await GetAsync<T>(endpoint, isRetry: true);
                }
            }

            return await ParseResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", $"GET {endpoint} exception", ex);
            return ApiResponse<T>.Fail(ErrorCodes.InternalError, ex.Message);
        }
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body, bool isRetry = false)
    {
        try
        {
            _logService.Info("AzureApiService", $"POST {endpoint} - HasToken: {!string.IsNullOrEmpty(_accessToken)}, IsRetry: {isRetry}");

            var response = body != null
                ? await _httpClient.PostAsJsonAsync(endpoint, body, JsonOptions)
                : await _httpClient.PostAsync(endpoint, null);

            _logService.Info("AzureApiService", $"POST {endpoint} - StatusCode: {(int)response.StatusCode} {response.StatusCode}");

            // Handle 401 - try to refresh token and retry once
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
            {
                _logService.Warning("AzureApiService", $"POST {endpoint} returned 401, attempting token refresh");
                if (await TryRefreshTokenAsync())
                {
                    return await PostAsync<T>(endpoint, body, isRetry: true);
                }
                _logService.Warning("AzureApiService", "Token refresh failed or returned null");
            }

            return await ParseResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", $"POST {endpoint} exception: {ex.Message}", ex);
            return ApiResponse<T>.Fail(ErrorCodes.InternalError, ex.Message);
        }
    }

    private async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body, bool isRetry = false)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, body, JsonOptions);

            // Handle 401 - try to refresh token and retry once
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
            {
                _logService.Warning("AzureApiService", $"PUT {endpoint} returned 401, attempting token refresh");
                if (await TryRefreshTokenAsync())
                {
                    return await PutAsync<T>(endpoint, body, isRetry: true);
                }
            }

            return await ParseResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", $"PUT {endpoint} exception", ex);
            return ApiResponse<T>.Fail(ErrorCodes.InternalError, ex.Message);
        }
    }

    private async Task<ApiResponse> DeleteAsync(string endpoint, bool isRetry = false)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            // Handle 401 - try to refresh token and retry once
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
            {
                _logService.Warning("AzureApiService", $"DELETE {endpoint} returned 401, attempting token refresh");
                if (await TryRefreshTokenAsync())
                {
                    return await DeleteAsync(endpoint, isRetry: true);
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return ApiResponse.Ok();
            }

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return ApiResponse.Ok();
            }

            var errorResponse = JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions);
            return errorResponse ?? ApiResponse.Fail(ErrorCodes.InternalError, "Unknown error");
        }
        catch (Exception ex)
        {
            _logService.Error("AzureApiService", $"DELETE {endpoint} exception", ex);
            return ApiResponse.Fail(ErrorCodes.InternalError, ex.Message);
        }
    }

    private async Task<ApiResponse<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        _logService.Info("AzureApiService", $"ParseResponse - StatusCode: {(int)response.StatusCode}, ContentLength: {content?.Length ?? 0}");

        // Handle non-success status codes
        if (!response.IsSuccessStatusCode)
        {
            _logService.Warning("AzureApiService", $"API error response ({(int)response.StatusCode}): {content}");

            // Check if it looks like HTML (error page)
            if (content?.TrimStart().StartsWith("<") == true)
            {
                return ApiResponse<T>.Fail(
                    $"HTTP_{(int)response.StatusCode}",
                    $"Server returned {response.StatusCode}. API may be unavailable.");
            }

            // Try to parse as error response
            try
            {
                var errorResult = JsonSerializer.Deserialize<ApiResponse<T>>(content ?? "", JsonOptions);
                if (errorResult != null)
                    return errorResult;
            }
            catch { }

            // Fallback error
            var preview = content?.Length > 100 ? content.Substring(0, 100) + "..." : content;
            return ApiResponse<T>.Fail($"HTTP_{(int)response.StatusCode}", preview ?? response.StatusCode.ToString());
        }

        if (string.IsNullOrEmpty(content))
        {
            return ApiResponse<T>.Ok(default!);
        }

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
            if (result != null && !result.Success)
            {
                _logService.Warning("AzureApiService", $"API returned error: Code={result.Error?.Code}, Message={result.Error?.Message}");
            }
            return result ?? ApiResponse<T>.Fail(ErrorCodes.InternalError, "Failed to parse response");
        }
        catch (JsonException ex)
        {
            _logService.Error("AzureApiService", $"Failed to parse response: {content}, Error: {ex.Message}");
            // Show first 200 chars of response to help diagnose
            var preview = content?.Length > 200 ? content.Substring(0, 200) + "..." : content;
            return ApiResponse<T>.Fail(ErrorCodes.InternalError, $"Invalid response: {preview}");
        }
    }

    #endregion
}
