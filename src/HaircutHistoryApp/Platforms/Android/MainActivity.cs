using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using HaircutHistoryApp.Platforms.Android.Services;

namespace HaircutHistoryApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static GoogleAuthService? GoogleAuthService { get; set; }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        try
        {
            // Handle Google Sign-In result
            if (GoogleAuthService != null && requestCode == GoogleAuthService.SignInRequestCode)
            {
                GoogleAuthService.HandleSignInResult(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnActivityResult error: {ex}");
        }
    }
}
