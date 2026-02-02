using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.ViewModels;

public partial class GlossaryViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<GlossaryGroup> _groups = new();

    [ObservableProperty]
    private GlossaryItem? _selectedItem;

    [ObservableProperty]
    private bool _showDetail;

    public GlossaryViewModel()
    {
        Title = "Glossary";
        LoadGroups();
    }

    private void LoadGroups()
    {
        Groups.Clear();

        // Add Areas group
        var areasGroup = new GlossaryGroup("Areas", "Different sections of the head");
        foreach (var item in GlossaryData.Areas)
        {
            areasGroup.Add(item);
        }
        Groups.Add(areasGroup);

        // Add Guard Sizes group
        var guardsGroup = new GlossaryGroup("Guard Sizes", "Clipper guard lengths");
        foreach (var item in GlossaryData.GuardSizes)
        {
            guardsGroup.Add(item);
        }
        Groups.Add(guardsGroup);

        // Add Techniques group
        var techniquesGroup = new GlossaryGroup("Techniques", "Cutting methods and styles");
        foreach (var item in GlossaryData.Techniques)
        {
            techniquesGroup.Add(item);
        }
        Groups.Add(techniquesGroup);
    }

    [RelayCommand]
    private void SelectItem(GlossaryItem item)
    {
        SelectedItem = item;
        ShowDetail = true;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        ShowDetail = false;
        SelectedItem = null;
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

/// <summary>
/// A grouped collection of glossary items for sectioned display.
/// </summary>
public class GlossaryGroup : ObservableCollection<GlossaryItem>
{
    public string Name { get; }
    public string Description { get; }

    public GlossaryGroup(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
