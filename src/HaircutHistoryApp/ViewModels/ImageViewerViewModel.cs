using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ImagePath), "imagePath")]
public partial class ImageViewerViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private ImageSource? _imageSource;

    public ImageViewerViewModel()
    {
        Title = "Photo";
    }

    partial void OnImagePathChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var decodedPath = Uri.UnescapeDataString(value);
            ImageSource = ImageSource.FromFile(decodedPath);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ShareImageAsync()
    {
        if (string.IsNullOrEmpty(ImagePath))
            return;

        var decodedPath = Uri.UnescapeDataString(ImagePath);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Share Photo",
            File = new ShareFile(decodedPath)
        });
    }
}
