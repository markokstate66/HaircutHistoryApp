using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class HaircutListPage : ContentPage
{
    public HaircutListPage(HaircutListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HaircutListViewModel vm)
        {
            vm.LoadHaircutsCommand.Execute(null);
        }
    }
}
