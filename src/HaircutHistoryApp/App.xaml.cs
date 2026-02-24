using HaircutHistoryApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HaircutHistoryApp;

public partial class App : Application
{
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		// Wire up token refresh for 401 handling
		WireUpTokenRefresh(serviceProvider);

		// Initialize theme service
		InitializeThemeAsync(serviceProvider);

		// Initialize ad service and preload interstitial
		InitializeAdsAsync(serviceProvider);
	}

	private async void InitializeThemeAsync(IServiceProvider serviceProvider)
	{
		try
		{
			var themeService = serviceProvider.GetService<IThemeService>();
			if (themeService != null)
			{
				await themeService.InitializeAsync();
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to initialize theme: {ex.Message}");
		}
	}

	private async void InitializeAdsAsync(IServiceProvider serviceProvider)
	{
		try
		{
			var adService = serviceProvider.GetService<IAdService>();
			if (adService != null)
			{
				await adService.InitializeAsync();

				// Preload interstitial ad for free users
				if (adService.ShouldShowAds)
				{
					adService.LoadInterstitialAd();
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to initialize ads: {ex.Message}");
		}
	}

	private void WireUpTokenRefresh(IServiceProvider serviceProvider)
	{
		try
		{
			var apiService = serviceProvider.GetService<IApiService>();
			var nativeAuthService = serviceProvider.GetService<INativeAuthService>();
			var logService = serviceProvider.GetService<ILogService>();

			if (apiService is AzureApiService azureApiService && nativeAuthService != null)
			{
				azureApiService.OnTokenRefreshNeeded += async () =>
				{
					try
					{
						logService?.Info("App", "Attempting token refresh via silent sign-in");

						var result = await nativeAuthService.TrySilentSignInAsync();

						if (result?.Success == true && !string.IsNullOrEmpty(result.IdToken))
						{
							// Update stored token
							Preferences.Set("NativeAuthIdToken", result.IdToken);
							logService?.Info("App", "Token refresh successful");
							return result.IdToken;
						}

						logService?.Warning("App", "Silent sign-in returned no token");
					}
					catch (Exception ex)
					{
						logService?.Error("App", "Token refresh failed", ex);
					}

					return null;
				};

				logService?.Info("App", "Token refresh callback wired up");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Failed to wire up token refresh: {ex.Message}");
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}