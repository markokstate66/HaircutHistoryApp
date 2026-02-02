using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Shared.DTOs;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// The response data (null on error).
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error details (null on success).
    /// </summary>
    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static ApiResponse<T> Fail(string code, string message) => new()
    {
        Success = false,
        Error = new ApiError { Code = code, Message = message }
    };
}

/// <summary>
/// Standard API response without data.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error details (null on success).
    /// </summary>
    [JsonPropertyName("error")]
    public ApiError? Error { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse Ok() => new() { Success = true };

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static ApiResponse Fail(string code, string message) => new()
    {
        Success = false,
        Error = new ApiError { Code = code, Message = message }
    };
}

/// <summary>
/// API error details.
/// </summary>
public class ApiError
{
    /// <summary>
    /// Error code (e.g., "UNAUTHORIZED", "NOT_FOUND", "LIMIT_EXCEEDED").
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Paginated list response.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The list of items.
    /// </summary>
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Total number of items (across all pages).
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Offset from the beginning.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;
}

/// <summary>
/// Common error codes.
/// </summary>
public static class ErrorCodes
{
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string LimitExceeded = "LIMIT_EXCEEDED";
    public const string PremiumRequired = "PREMIUM_REQUIRED";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string InternalError = "INTERNAL_ERROR";
}
