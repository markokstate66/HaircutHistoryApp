using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class ProfileDetailPage : ContentPage
{
    public ProfileDetailPage(ProfileDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
