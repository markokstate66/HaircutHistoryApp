using HaircutHistoryApp.Models;
using HaircutHistoryApp.Services;

namespace HaircutHistoryApp.Controls;

public partial class GlossaryInfoButton : ContentView
{
    public static readonly BindableProperty CategoryProperty =
        BindableProperty.Create(nameof(Category), typeof(GlossaryCategory?), typeof(GlossaryInfoButton), null);

    public static readonly BindableProperty ItemNameProperty =
        BindableProperty.Create(nameof(ItemName), typeof(string), typeof(GlossaryInfoButton), null);

    /// <summary>
    /// The glossary category to show. If set, navigates to the category list.
    /// </summary>
    public GlossaryCategory? Category
    {
        get => (GlossaryCategory?)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    /// <summary>
    /// The specific item name to show. If set, shows a popup with that item's details.
    /// </summary>
    public string? ItemName
    {
        get => (string?)GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public GlossaryInfoButton()
    {
        InitializeComponent();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        // If a specific item name is provided, show inline popup
        if (!string.IsNullOrEmpty(ItemName))
        {
            var item = GlossaryData.GetByName(ItemName);
            if (item != null)
            {
                await ShowItemPopup(item);
                return;
            }
        }

        // Otherwise navigate to the glossary page with the category
        if (Category.HasValue)
        {
            await Shell.Current.GoToAsync($"glossary?category={Category.Value}");
        }
        else
        {
            await Shell.Current.GoToAsync("glossary");
        }
    }

    private async Task ShowItemPopup(GlossaryItem item)
    {
        var description = item.Description;
        if (!string.IsNullOrEmpty(item.ImageSource))
        {
            description = $"{item.Description}\n\n(Tap 'Learn More' to see image)";
        }

        var result = await Shell.Current.DisplayAlertAsync(
            item.Name,
            item.Description,
            "Learn More",
            "OK");

        if (result)
        {
            // Navigate to glossary with the item's category
            var category = GetCategoryForItem(item);
            if (category.HasValue)
            {
                await Shell.Current.GoToAsync($"glossary?category={category.Value}");
            }
            else
            {
                await Shell.Current.GoToAsync("glossary");
            }
        }
    }

    private GlossaryCategory? GetCategoryForItem(GlossaryItem item)
    {
        if (GlossaryData.Areas.Any(x => x.Name == item.Name))
            return GlossaryCategory.Areas;
        if (GlossaryData.GuardSizes.Any(x => x.Name == item.Name))
            return GlossaryCategory.GuardSizes;
        if (GlossaryData.Techniques.Any(x => x.Name == item.Name))
            return GlossaryCategory.Techniques;
        return null;
    }
}
