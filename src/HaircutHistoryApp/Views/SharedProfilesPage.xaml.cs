using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class SharedProfilesPage : ContentPage
{
    public SharedProfilesPage(SharedProfilesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SharedProfilesViewModel vm)
        {
            vm.LoadProfilesCommand.Execute(null);
        }
    }
}
