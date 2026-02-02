using HaircutHistoryApp.Views;

namespace HaircutHistoryApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for sub-page navigation (not top-level flyout items)
        Routing.RegisterRoute("addProfile", typeof(AddEditProfilePage));
        Routing.RegisterRoute("editProfile", typeof(AddEditProfilePage));
        Routing.RegisterRoute("profileDetail", typeof(ProfileDetailPage));
        Routing.RegisterRoute("qrShare", typeof(QRSharePage));
        Routing.RegisterRoute("imageViewer", typeof(ImageViewerPage));
        Routing.RegisterRoute("achievements", typeof(AchievementsPage));
        Routing.RegisterRoute("haircutList", typeof(HaircutListPage));
        Routing.RegisterRoute("addHaircut", typeof(AddEditHaircutPage));
        Routing.RegisterRoute("editHaircut", typeof(AddEditHaircutPage));
        Routing.RegisterRoute("cuttingGuide", typeof(CuttingGuidePage));
        Routing.RegisterRoute("themeSelection", typeof(ThemeSelectionPage));
    }
}
