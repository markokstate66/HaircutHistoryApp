using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class GlossaryPage : ContentPage
{
    public GlossaryPage(GlossaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
