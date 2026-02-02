using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HaircutHistoryApp.Api.Middleware;

/// <summary>
/// Middleware to validate Firebase JWT tokens.
/// Validates tokens against Google's public keys and Firebase issuer/audience.
/// </summary>
public class AuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<AuthMiddleware> _logger;
    private readonly string _firebaseProjectId;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    // Google's public keys endpoint for Firebase token verification
    private const string GooglePublicKeysUrl =
        "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";

    // Cache for signing keys
    private static IEnumerable<SecurityKey>? _signingKeys;
    private static DateTime _keysLastFetched = DateTime.MinValue;
    private static readonly TimeSpan KeysCacheDuration = TimeSpan.FromHours(6);
    private static readonly SemaphoreSlim _keysLock = new(1, 1);

    public AuthMiddleware(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<AuthMiddleware>();
        _firebaseProjectId = configuration["Firebase:ProjectId"] ?? "haircut-history";
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Get the function name to check if auth is required
        var functionName = context.FunctionDefinition.Name;

        // Skip auth for health check
        if (functionName.Equals("Health", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Get the Authorization header
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            await next(context);
            return;
        }

        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Missing or invalid Authorization header");
            context.Items["AuthError"] = "Missing or invalid Authorization header";
            context.Items["UserId"] = null;
            await next(context);
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            // Validate the Firebase JWT token
            var principal = await ValidateFirebaseTokenAsync(token);

            if (principal != null)
            {
                // Extract user info from Firebase JWT claims
                // Firebase uses "user_id" or "sub" for the user ID
                var userId = principal.FindFirst("user_id")?.Value
                    ?? principal.FindFirst("sub")?.Value;
                var email = principal.FindFirst("email")?.Value;
                var name = principal.FindFirst("name")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Token does not contain user ID");
                    context.Items["AuthError"] = "Invalid token: missing user ID";
                    context.Items["UserId"] = null;
                }
                else
                {
                    context.Items["UserId"] = userId;
                    context.Items["UserEmail"] = email;
                    context.Items["UserName"] = name;
                    context.Items["AuthError"] = null;
                    _logger.LogDebug("Authenticated user: {UserId}, {Email}", userId, email);
                }
            }
            else
            {
                context.Items["AuthError"] = "Token validation failed";
                context.Items["UserId"] = null;
            }
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token has expired");
            context.Items["AuthError"] = "Token expired";
            context.Items["UserId"] = null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            context.Items["AuthError"] = "Invalid token";
            context.Items["UserId"] = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            context.Items["AuthError"] = "Token validation error";
            context.Items["UserId"] = null;
        }

        await next(context);
    }

    /// <summary>
    /// Validates a Firebase ID token and returns the claims principal.
    /// </summary>
    private async Task<System.Security.Claims.ClaimsPrincipal?> ValidateFirebaseTokenAsync(string token)
    {
        // Get or refresh signing keys
        var signingKeys = await GetSigningKeysAsync();
        if (signingKeys == null || !signingKeys.Any())
        {
            _logger.LogError("Failed to retrieve Firebase signing keys");
            return null;
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{_firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = _firebaseProjectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
        };

        var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal;
    }

    /// <summary>
    /// Gets the Firebase/Google signing keys, using a cached version if available.
    /// </summary>
    private async Task<IEnumerable<SecurityKey>?> GetSigningKeysAsync()
    {
        // Check if we have cached keys that are still valid
        if (_signingKeys != null && DateTime.UtcNow - _keysLastFetched < KeysCacheDuration)
        {
            return _signingKeys;
        }

        // Acquire lock to refresh keys
        await _keysLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_signingKeys != null && DateTime.UtcNow - _keysLastFetched < KeysCacheDuration)
            {
                return _signingKeys;
            }

            // Fetch new keys from Google
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(GooglePublicKeysUrl);
            var keysDict = JsonSerializer.Deserialize<Dictionary<string, string>>(response);

            if (keysDict == null || keysDict.Count == 0)
            {
                _logger.LogError("Failed to parse Google public keys");
                return _signingKeys; // Return cached keys if available
            }

            var keys = new List<SecurityKey>();
            foreach (var (kid, certPem) in keysDict)
            {
                try
                {
                    var cert = new X509Certificate2(System.Text.Encoding.UTF8.GetBytes(certPem));
                    keys.Add(new X509SecurityKey(cert, kid));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse certificate for key {KeyId}", kid);
                }
            }

            _signingKeys = keys;
            _keysLastFetched = DateTime.UtcNow;
            _logger.LogInformation("Refreshed Firebase signing keys, {Count} keys loaded", keys.Count);

            return _signingKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Firebase signing keys");
            return _signingKeys; // Return cached keys if available
        }
        finally
        {
            _keysLock.Release();
        }
    }
}

/// <summary>
/// Extension methods for FunctionContext.
/// </summary>
public static class FunctionContextExtensions
{
    /// <summary>
    /// Gets the authenticated user's ID (Firebase UID) from the context.
    /// </summary>
    public static string? GetUserId(this FunctionContext context)
    {
        return context.Items.TryGetValue("UserId", out var userId) ? userId as string : null;
    }

    /// <summary>
    /// Gets the authenticated user's email from the context.
    /// </summary>
    public static string? GetUserEmail(this FunctionContext context)
    {
        return context.Items.TryGetValue("UserEmail", out var email) ? email as string : null;
    }

    /// <summary>
    /// Gets the authenticated user's name from the context.
    /// </summary>
    public static string? GetUserName(this FunctionContext context)
    {
        return context.Items.TryGetValue("UserName", out var name) ? name as string : null;
    }

    /// <summary>
    /// Gets the auth error if authentication failed.
    /// </summary>
    public static string? GetAuthError(this FunctionContext context)
    {
        return context.Items.TryGetValue("AuthError", out var error) ? error as string : null;
    }

    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    public static bool IsAuthenticated(this FunctionContext context)
    {
        return context.GetUserId() != null;
    }
}
