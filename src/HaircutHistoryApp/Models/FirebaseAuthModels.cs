using System.Text.Json.Serialization;

namespace HaircutHistoryApp.Models;

/// <summary>
/// Response from Firebase sign-in/sign-up endpoints.
/// </summary>
public class FirebaseAuthResponse
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expiresIn")]
    public string ExpiresIn { get; set; } = "3600";

    [JsonPropertyName("localId")]
    public string LocalId { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("registered")]
    public bool? Registered { get; set; }

    // For social login
    [JsonPropertyName("federatedId")]
    public string? FederatedId { get; set; }

    [JsonPropertyName("providerId")]
    public string? ProviderId { get; set; }

    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }
}

/// <summary>
/// Response from Firebase token refresh endpoint.
/// </summary>
public class FirebaseRefreshResponse
{
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = "3600";

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = string.Empty;
}

/// <summary>
/// Firebase error response.
/// </summary>
public class FirebaseErrorResponse
{
    [JsonPropertyName("error")]
    public FirebaseError? Error { get; set; }
}

public class FirebaseError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<FirebaseErrorDetail>? Errors { get; set; }
}

public class FirebaseErrorDetail
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request for Firebase sign-in with IdP (Google/Apple).
/// </summary>
public class FirebaseSignInWithIdpRequest
{
    [JsonPropertyName("postBody")]
    public string PostBody { get; set; } = string.Empty;

    [JsonPropertyName("requestUri")]
    public string RequestUri { get; set; } = "http://localhost";

    [JsonPropertyName("returnIdpCredential")]
    public bool ReturnIdpCredential { get; set; } = true;

    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

/// <summary>
/// Request for Firebase email/password sign-in.
/// </summary>
public class FirebaseSignInRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

/// <summary>
/// Request for Firebase email/password sign-up.
/// </summary>
public class FirebaseSignUpRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}

/// <summary>
/// Request for Firebase token refresh.
/// </summary>
public class FirebaseRefreshRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "refresh_token";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request for Firebase profile update.
/// </summary>
public class FirebaseUpdateProfileRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("returnSecureToken")]
    public bool ReturnSecureToken { get; set; } = true;
}
