using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using HaircutHistoryApp.Services;
using AndroidTask = Android.Gms.Tasks.Task;
using Task = System.Threading.Tasks.Task;

namespace HaircutHistoryApp.Platforms.Android.Services;

/// <summary>
/// Native Google Sign-In implementation for Android.
/// Provides the native account picker UI.
/// </summary>
public class GoogleAuthService : Java.Lang.Object, INativeAuthService
{
    private const int RC_SIGN_IN = 9001;

    // Web client ID from Firebase Console (required for ID token)
    private const string WebClientId = "517009667256-7vs9vo255iga71ks9a1m9fam1ojkbtue.apps.googleusercontent.com";

    private GoogleSignInClient? _googleSignInClient;
    private TaskCompletionSource<NativeAuthResult>? _signInTcs;

    public GoogleAuthService()
    {
        // Don't configure immediately - Platform.CurrentActivity may not be available yet
    }

    private void EnsureConfigured()
    {
        if (_googleSignInClient != null)
            return;

        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                System.Diagnostics.Debug.WriteLine("GoogleAuthService: CurrentActivity is null");
                return;
            }

            var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken(WebClientId)
                .RequestEmail()
                .RequestProfile()
                .Build();

            _googleSignInClient = GoogleSignIn.GetClient(activity, gso);
            System.Diagnostics.Debug.WriteLine("GoogleAuthService: Configured successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GoogleAuthService: Configuration error - {ex.Message}");
        }
    }

    public async Task<NativeAuthResult> SignInAsync()
    {
        try
        {
            // Ensure client is configured
            EnsureConfigured();

            if (_googleSignInClient == null)
            {
                return new NativeAuthResult
                {
                    Success = false,
                    Error = "Google Sign-In not available"
                };
            }

            _signInTcs = new TaskCompletionSource<NativeAuthResult>();

            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                return new NativeAuthResult
                {
                    Success = false,
                    Error = "No activity available"
                };
            }

            // Register for activity result
            var intent = _googleSignInClient.SignInIntent;
            activity.StartActivityForResult(intent, RC_SIGN_IN);

            // Wait for result (will be set by HandleSignInResult)
            return await _signInTcs.Task;
        }
        catch (Exception ex)
        {
            return new NativeAuthResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<NativeAuthResult?> TrySilentSignInAsync()
    {
        try
        {
            EnsureConfigured();

            if (_googleSignInClient == null)
                return null;

            var account = await SilentSignInAsync();

            if (account != null)
            {
                return new NativeAuthResult
                {
                    Success = true,
                    Email = account.Email,
                    DisplayName = account.DisplayName,
                    UserId = account.Id,
                    IdToken = account.IdToken,
                    PhotoUrl = account.PhotoUrl?.ToString(),
                    Provider = "Google"
                };
            }
        }
        catch (ApiException ex) when (ex.StatusCode == CommonStatusCodes.SignInRequired)
        {
            // Silent sign-in failed, need interactive
        }
        catch (Exception)
        {
            // Silent sign-in failed
        }

        return null;
    }

    private Task<GoogleSignInAccount?> SilentSignInAsync()
    {
        var tcs = new TaskCompletionSource<GoogleSignInAccount?>();

        if (_googleSignInClient == null)
        {
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        try
        {
            _googleSignInClient.SilentSignIn()
                .AddOnSuccessListener(new OnSuccessListener(result =>
                {
                    tcs.TrySetResult(result as GoogleSignInAccount);
                }))
                .AddOnFailureListener(new OnFailureListener(ex =>
                {
                    tcs.TrySetResult(null);
                }));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SilentSignIn error: {ex}");
            tcs.TrySetResult(null);
        }

        return tcs.Task;
    }

    public Task SignOutAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        if (_googleSignInClient == null)
        {
            tcs.TrySetResult(true);
            return tcs.Task;
        }

        _googleSignInClient.SignOut()
            .AddOnCompleteListener(new OnCompleteListener(() =>
            {
                tcs.TrySetResult(true);
            }));

        return tcs.Task;
    }

    /// <summary>
    /// Called from MainActivity when sign-in activity returns.
    /// </summary>
    public void HandleSignInResult(Intent? data)
    {
        try
        {
            var task = GoogleSignIn.GetSignedInAccountFromIntent(data);
            var account = task.GetResult(Java.Lang.Class.FromType(typeof(ApiException))) as GoogleSignInAccount;

            if (account != null)
            {
                _signInTcs?.TrySetResult(new NativeAuthResult
                {
                    Success = true,
                    Email = account.Email,
                    DisplayName = account.DisplayName,
                    UserId = account.Id,
                    IdToken = account.IdToken,
                    PhotoUrl = account.PhotoUrl?.ToString(),
                    Provider = "Google"
                });
            }
            else
            {
                _signInTcs?.TrySetResult(new NativeAuthResult
                {
                    Success = false,
                    Error = "No account returned"
                });
            }
        }
        catch (ApiException ex)
        {
            // Detailed error logging
            System.Diagnostics.Debug.WriteLine($"=== GOOGLE SIGN-IN ERROR ===");
            System.Diagnostics.Debug.WriteLine($"Status Code: {ex.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Status Message: {ex.StatusMessage}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Web Client ID: {WebClientId}");
            System.Diagnostics.Debug.WriteLine($"=== END ERROR ===");

            var errorMessage = ex.StatusCode switch
            {
                CommonStatusCodes.Canceled => "Sign-in cancelled",
                CommonStatusCodes.NetworkError => "Network error. Please check your connection.",
                10 => "Developer error: Check SHA-1/package name in Google Cloud Console",
                12500 => "Sign-in failed: OAuth config error. Update SHA-1 in Google Cloud Console to: 53:4E:6D:DF:B6:98:7D:4F:06:2D:EC:2A:7C:03:D1:5C:66:B5:44:35",
                12501 => "Sign-in cancelled",
                12502 => "Sign-in currently in progress",
                _ => $"Sign-in failed (code: {ex.StatusCode}): {ex.Message}"
            };

            _signInTcs?.TrySetResult(new NativeAuthResult
            {
                Success = false,
                Error = errorMessage
            });
        }
        catch (Exception ex)
        {
            _signInTcs?.TrySetResult(new NativeAuthResult
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Request code for sign-in activity.
    /// </summary>
    public static int SignInRequestCode => RC_SIGN_IN;
}

// Helper listener classes
public class OnSuccessListener : Java.Lang.Object, global::Android.Gms.Tasks.IOnSuccessListener
{
    private readonly Action<Java.Lang.Object?> _onSuccess;

    public OnSuccessListener(Action<Java.Lang.Object?> onSuccess)
    {
        _onSuccess = onSuccess;
    }

    public void OnSuccess(Java.Lang.Object? result)
    {
        _onSuccess(result);
    }
}

public class OnFailureListener : Java.Lang.Object, global::Android.Gms.Tasks.IOnFailureListener
{
    private readonly Action<Exception> _onFailure;

    public OnFailureListener(Action<Exception> onFailure)
    {
        _onFailure = onFailure;
    }

    public void OnFailure(Java.Lang.Exception e)
    {
        _onFailure(new Exception(e.Message));
    }
}

public class OnCompleteListener : Java.Lang.Object, global::Android.Gms.Tasks.IOnCompleteListener
{
    private readonly Action _onComplete;

    public OnCompleteListener(Action onComplete)
    {
        _onComplete = onComplete;
    }

    public void OnComplete(AndroidTask task)
    {
        _onComplete();
    }
}
