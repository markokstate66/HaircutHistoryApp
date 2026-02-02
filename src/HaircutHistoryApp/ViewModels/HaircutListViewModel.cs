using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

[QueryProperty(nameof(ProfileId), "profileId")]
[QueryProperty(nameof(ProfileName), "profileName")]
public partial class HaircutListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<HaircutRecord> _haircuts = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasHaircuts;

    public HaircutListViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Haircut History";
    }

    partial void OnProfileIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            LoadHaircutsCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadHaircutsAsync()
    {
        if (string.IsNullOrEmpty(ProfileId))
            return;

        await ExecuteAsync(async () =>
        {
            var records = await _dataService.GetHaircutRecordsAsync(ProfileId);

            Haircuts.Clear();
            foreach (var record in records.OrderByDescending(r => r.Date))
            {
                Haircuts.Add(record);
            }

            HasHaircuts = Haircuts.Count > 0;
            Title = string.IsNullOrEmpty(ProfileName) ? "Haircut History" : $"{ProfileName}'s Haircuts";
        });

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadHaircutsAsync();
    }

    [RelayCommand]
    private async Task AddHaircutAsync()
    {
        await Shell.Current.GoToAsync($"addHaircut?profileId={ProfileId}&profileName={Uri.EscapeDataString(ProfileName)}");
    }

    [RelayCommand]
    private async Task ViewHaircutAsync(HaircutRecord haircut)
    {
        if (haircut == null)
            return;

        await Shell.Current.GoToAsync($"editHaircut?profileId={ProfileId}&profileName={Uri.EscapeDataString(ProfileName)}&recordId={haircut.Id}");
    }

    [RelayCommand]
    private async Task DeleteHaircutAsync(HaircutRecord haircut)
    {
        if (haircut == null)
            return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Haircut",
            "Are you sure you want to delete this haircut record?",
            "Delete", "Cancel");

        if (!confirm)
            return;

        await ExecuteAsync(async () =>
        {
            var success = await _dataService.DeleteHaircutRecordAsync(ProfileId, haircut.Id);

            if (success)
            {
                Haircuts.Remove(haircut);
                HasHaircuts = Haircuts.Count > 0;
            }
            else
            {
                var errorDetail = _dataService.LastError ?? "Unknown error";
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to delete haircut record.\n\n{errorDetail}", "OK");
            }
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
