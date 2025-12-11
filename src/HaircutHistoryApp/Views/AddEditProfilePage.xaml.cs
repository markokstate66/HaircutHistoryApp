using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class AddEditProfilePage : ContentPage
{
    public AddEditProfilePage(AddEditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
