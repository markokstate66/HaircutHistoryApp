using CommunityToolkit.Maui;
using HaircutHistoryApp.Services;
using HaircutHistoryApp.Services.Analytics;
using HaircutHistoryApp.ViewModels;
using HaircutHistoryApp.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;
#if ANDROID || IOS
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
        builder.Services.AddSingleton<IPlayFabService, PlayFabService>();
        builder.Services.AddSingleton<IProfilePictureService, ProfilePictureService>();
        builder.Services.AddSingleton<IAuthService, PlayFabAuthService>();
        builder.Services.AddSingleton<IDataService, PlayFabDataService>();
        builder.Services.AddSingleton<IImageService, ImageService>();
        builder.Services.AddSingleton<IQRService, QRService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
        builder.Services.AddSingleton<IAdService, AdService>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ProfileListViewModel>();
        builder.Services.AddTransient<ProfileDetailViewModel>();
        builder.Services.AddTransient<AddEditProfileViewModel>();
        builder.Services.AddTransient<QRShareViewModel>();
        builder.Services.AddTransient<QRScanViewModel>();
        builder.Services.AddTransient<BarberDashboardViewModel>();
        builder.Services.AddTransient<ClientViewViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<ImageViewerViewModel>();
        builder.Services.AddTransient<AchievementsViewModel>();
        builder.Services.AddTransient<PremiumViewModel>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ProfileListPage>();
        builder.Services.AddTransient<ProfileDetailPage>();
        builder.Services.AddTransient<AddEditProfilePage>();
        builder.Services.AddTransient<QRSharePage>();
        builder.Services.AddTransient<QRScanPage>();
        builder.Services.AddTransient<BarberDashboardPage>();
        builder.Services.AddTransient<ClientViewPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ImageViewerPage>();
        builder.Services.AddTransient<AchievementsPage>();
        builder.Services.AddTransient<PremiumPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
