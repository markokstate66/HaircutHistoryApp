namespace HaircutHistoryApp.Controls;

public partial class LoadingOverlay : ContentView
{
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingOverlay), false);

    public static readonly BindableProperty LoadingTextProperty =
        BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(LoadingOverlay), "Loading...");

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();
    }
}
