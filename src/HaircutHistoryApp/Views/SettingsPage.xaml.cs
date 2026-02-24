using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

#if DEBUG
        DebugOptionsFrame.IsVisible = true;
        DebugPremiumSwitch.Toggled += OnDebugPremiumToggled;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadSettingsCommand.Execute(null);

#if DEBUG
        DebugPremiumSwitch.IsToggled = _viewModel.IsDebugPremium;
#endif
    }

#if DEBUG
    private async void OnDebugPremiumToggled(object? sender, ToggledEventArgs e)
    {
        if (_viewModel.IsDebugPremium != e.Value)
        {
            await _viewModel.ToggleDebugPremiumCommand.ExecuteAsync(null);
        }
    }
#endif
}
