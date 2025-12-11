using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class QRSharePage : ContentPage
{
    public QRSharePage(QRShareViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
