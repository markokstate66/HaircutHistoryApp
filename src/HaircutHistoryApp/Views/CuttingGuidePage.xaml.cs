using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class CuttingGuidePage : ContentPage
{
    private readonly CuttingGuideViewModel _viewModel;

    public CuttingGuidePage(CuttingGuideViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}
