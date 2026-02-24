using CommunityToolkit.Maui;
using HaircutHistoryApp.Services;
using HaircutHistoryApp.Services.Analytics;
using HaircutHistoryApp.ViewModels;
using HaircutHistoryApp.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;
#if ANDROID
using HaircutHistoryApp.Platforms.Android.Services;
using Plugin.MauiMTAdmob;
#elif IOS
using Plugin.MauiMTAdmob;
#endif

namespace HaircutHistoryApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
#if ANDROID || IOS
            .UseMauiMTAdmob()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register HttpClientFactory for proper HTTP client lifecycle management
        builder.Services.AddHttpClient();

        // Register Services
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<ILogService, LogService>();
        builder.Services.AddSingleton<IProfilePictureService, ProfilePictureService>();
        builder.Services.AddSingleton<IImageService, ImageService>();
        builder.Services.AddSingleton<IQRService, QRService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
        builder.Services.AddSingleton<IAdService, AdService>();

        // Register Native Auth Service (platform-specific) - must be before FirebaseAuthService
#if ANDROID
        builder.Services.AddSingleton<INativeAuthService>(sp =>
        {
            var service = new GoogleAuthService();
            MainActivity.GoogleAuthService = service;
            return service;
        });
#else
        builder.Services.AddSingleton<INativeAuthService, DefaultNativeAuthService>();
#endif

        // Register Azure/Firebase Services
        builder.Services.AddSingleton<IApiService, AzureApiService>();
        builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();

        // Register SQLite and Sync services for local caching
        builder.Services.AddSingleton<ISqliteService, SqliteService>();
        builder.Services.AddSingleton<AzureDataService>(); // Keep as concrete type for CachedDataService
        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<IDataService, CachedDataService>(); // Replace with cached version

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ProfileListViewModel>();
        builder.Services.AddTransient<ProfileDetailViewModel>();
        builder.Services.AddTransient<AddEditProfileViewModel>();
        builder.Services.AddTransient<QRShareViewModel>();
        builder.Services.AddTransient<QRScanViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<ImageViewerViewModel>();
        builder.Services.AddTransient<AchievementsViewModel>();
        builder.Services.AddTransient<PremiumViewModel>();
        builder.Services.AddTransient<HaircutListViewModel>();
        builder.Services.AddTransient<AddEditHaircutViewModel>();
        builder.Services.AddTransient<SharedProfilesViewModel>();
        builder.Services.AddTransient<GlossaryViewModel>();
        builder.Services.AddTransient<CuttingGuideViewModel>();
        builder.Services.AddTransient<ThemeSelectionViewModel>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ProfileListPage>();
        builder.Services.AddTransient<ProfileDetailPage>();
        builder.Services.AddTransient<AddEditProfilePage>();
        builder.Services.AddTransient<QRSharePage>();
        builder.Services.AddTransient<QRScanPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ImageViewerPage>();
        builder.Services.AddTransient<AchievementsPage>();
        builder.Services.AddTransient<PremiumPage>();
        builder.Services.AddTransient<HaircutListPage>();
        builder.Services.AddTransient<AddEditHaircutPage>();
        builder.Services.AddTransient<SharedProfilesPage>();
        builder.Services.AddTransient<GlossaryPage>();
        builder.Services.AddTransient<CuttingGuidePage>();
        builder.Services.AddTransient<ThemeSelectionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
