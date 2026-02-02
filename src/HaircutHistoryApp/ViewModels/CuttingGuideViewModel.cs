using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ProfileId), "profileId")]
public partial class CuttingGuideViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private bool _isDisposed;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CuttingStep> _steps = new();

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private bool _allComplete;

    [ObservableProperty]
    private string _generalNotes = string.Empty;

    public CuttingGuideViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Cutting Guide";
    }

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadProfileCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (string.IsNullOrEmpty(ProfileId))
            return;

        await ExecuteAsync(async () =>
        {
            var profile = await _dataService.GetProfileAsync(ProfileId);
            if (profile == null)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Profile not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ProfileName = profile.Name;
            Title = $"Guide: {profile.Name}";

            // Load the latest haircut to get measurements
            var haircuts = await _dataService.GetHaircutRecordsAsync(ProfileId);
            var latestHaircut = haircuts.FirstOrDefault();

            if (latestHaircut == null)
            {
                await Shell.Current.DisplayAlertAsync("No Haircuts",
                    "Add a haircut record first to use the cutting guide.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            GeneralNotes = latestHaircut.Notes ?? string.Empty;

            Steps.Clear();
            var orderedMeasurements = latestHaircut.Measurements
                .OrderBy(m => m.StepOrder)
                .ToList();

            foreach (var m in orderedMeasurements)
            {
                var step = new CuttingStep
                {
                    StepNumber = m.StepOrder > 0 ? m.StepOrder : Steps.Count + 1,
                    Area = m.Area,
                    GuardSize = m.GuardSize,
                    Technique = m.Technique,
                    Notes = m.Notes
                };
                step.PropertyChanged += OnStepPropertyChanged;
                Steps.Add(step);
            }

            TotalCount = Steps.Count;
            UpdateProgress();
        });
    }

    private void OnStepPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isDisposed) return;

        if (e.PropertyName == nameof(CuttingStep.IsComplete))
        {
            UpdateProgress();
        }
    }

    private void UpdateProgress()
    {
        if (_isDisposed) return;

        CompletedCount = Steps.Count(s => s.IsComplete);
        ProgressPercent = TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;
        AllComplete = CompletedCount == TotalCount && TotalCount > 0;
    }

    private void Cleanup()
    {
        _isDisposed = true;
        foreach (var step in Steps)
        {
            step.PropertyChanged -= OnStepPropertyChanged;
        }
    }

    [RelayCommand]
    private void ToggleStep(CuttingStep step)
    {
        if (step != null)
        {
            step.IsComplete = !step.IsComplete;
        }
    }

    [RelayCommand]
    private void ResetAll()
    {
        foreach (var step in Steps)
        {
            step.IsComplete = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        Cleanup();
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task EditProfileAsync()
    {
        Cleanup();
        await Shell.Current.GoToAsync($"editProfile?profileId={ProfileId}");
    }

    /// <summary>
    /// Called when the page is disappearing to cleanup event handlers.
    /// </summary>
    public void OnDisappearing()
    {
        Cleanup();
    }
}

public partial class CuttingStep : ObservableObject
{
    [ObservableProperty]
    private int _stepNumber;

    [ObservableProperty]
    private string _area = string.Empty;

    [ObservableProperty]
    private string? _guardSize;

    [ObservableProperty]
    private string? _technique;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isComplete;

    /// <summary>
    /// Formatted guard size display (e.g., "#2" or "Scissors")
    /// </summary>
    public string GuardDisplay
    {
        get
        {
            if (string.IsNullOrEmpty(GuardSize)) return "";
            if (GuardSize.StartsWith("0") || char.IsDigit(GuardSize[0]))
                return $"#{GuardSize}";
            return GuardSize;
        }
    }

    /// <summary>
    /// Combined technique and guard for compact display
    /// </summary>
    public string DetailsDisplay
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(GuardSize))
                parts.Add(GuardDisplay);
            if (!string.IsNullOrEmpty(Technique))
                parts.Add(Technique);
            return string.Join(" • ", parts);
        }
    }

    public bool HasNotes => !string.IsNullOrEmpty(Notes);
}
