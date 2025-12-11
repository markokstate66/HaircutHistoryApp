using HaircutHistoryApp.ViewModels;

namespace HaircutHistoryApp.Views;

public partial class ImageViewerPage : ContentPage
{
    public ImageViewerPage(ImageViewerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
