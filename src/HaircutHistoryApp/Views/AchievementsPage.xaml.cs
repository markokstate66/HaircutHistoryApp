using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class AchievementsPage : ContentPage
{
    private readonly AchievementsViewModel _viewModel;

    public AchievementsPage(AchievementsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadAchievementsCommand.Execute(null);
    }
}
