using HaircutHistoryApp.Views;

namespace HaircutHistoryApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("addProfile", typeof(AddEditProfilePage));
        Routing.RegisterRoute("editProfile", typeof(AddEditProfilePage));
        Routing.RegisterRoute("profileDetail", typeof(ProfileDetailPage));
        Routing.RegisterRoute("qrShare", typeof(QRSharePage));
        Routing.RegisterRoute("qrScan", typeof(QRScanPage));
        Routing.RegisterRoute("clientView", typeof(ClientViewPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute("imageViewer", typeof(ImageViewerPage));
        Routing.RegisterRoute("achievements", typeof(AchievementsPage));
        Routing.RegisterRoute("premium", typeof(PremiumPage));
    }
}
