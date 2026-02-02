using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class AddEditHaircutPage : ContentPage
{
    public AddEditHaircutPage(AddEditHaircutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
