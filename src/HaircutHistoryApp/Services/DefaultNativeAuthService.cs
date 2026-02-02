namespace HaircutHistoryApp.Services;

/// <summary>
/// Default implementation for platforms without native sign-in.
/// Falls back to MSAL/web-based auth.
/// </summary>
public class DefaultNativeAuthService : INativeAuthService
{
    public Task<NativeAuthResult> SignInAsync()
    {
        // Return failure - will fall back to MSAL
        return Task.FromResult(new NativeAuthResult
        {
            Success = false,
            Error = "Native sign-in not available on this platform"
        });
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }

    public Task<NativeAuthResult?> TrySilentSignInAsync()
    {
        return Task.FromResult<NativeAuthResult?>(null);
    }
}
