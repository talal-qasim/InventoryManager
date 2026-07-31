using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManager.Core.Entities;
using InventoryManager.Core.Interfaces;

namespace InventoryManager.App.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string newCategoryName = string.Empty;

    [ObservableProperty]
    private string newCategoryDescription = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private Category? selectedCategory;

    public CategoriesViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories.Clear();
        var categories = await _categoryService.GetAllAsync();
        foreach (var category in categories)
            Categories.Add(category);
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        ErrorMessage = string.Empty;

        var category = new Category
        {
            Name = NewCategoryName,
            Description = NewCategoryDescription
        };

        var result = await _categoryService.AddAsync(category);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to add category.";
            return;
        }

        NewCategoryName = string.Empty;
        NewCategoryDescription = string.Empty;
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task DeactivateCategory()
    {
        if (SelectedCategory is null)
            return;

        ErrorMessage = string.Empty;
        var result = await _categoryService.DeactivateAsync(SelectedCategory.Id);

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Unable to deactivate category.";
            return;
        }

        await LoadCategoriesAsync();
    }
}