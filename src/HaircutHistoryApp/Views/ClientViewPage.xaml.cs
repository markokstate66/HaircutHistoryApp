using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class ClientViewPage : ContentPage
{
    public ClientViewPage(ClientViewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
