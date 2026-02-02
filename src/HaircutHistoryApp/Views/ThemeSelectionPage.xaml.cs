using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class ThemeSelectionPage : ContentPage
{
    private readonly ThemeSelectionViewModel _viewModel;

    public ThemeSelectionPage(ThemeSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadThemesCommand.Execute(null);
    }
}
