using CommunityToolkit.Maui;
using HaircutHistoryApp.Services;
using HaircutHistoryApp.Services.Analytics;
using HaircutHistoryApp.ViewModels;
using HaircutHistoryApp.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<IPlayFabService, PlayFabService>();
        builder.Services.AddSingleton<IAuthService, PlayFabAuthService>();
        builder.Services.AddSingleton<IDataService, PlayFabDataService>();
        builder.Services.AddSingleton<IImageService, ImageService>();
        builder.Services.AddSingleton<IQRService, QRService>();

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
