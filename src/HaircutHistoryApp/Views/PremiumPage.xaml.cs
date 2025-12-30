using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class PremiumPage : ContentPage
{
    public PremiumPage(PremiumViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is PremiumViewModel vm)
        {
            vm.LoadProductsCommand.Execute(null);
        }
    }
}
