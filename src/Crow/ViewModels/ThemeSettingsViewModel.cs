using Crow.Models;
using Crow.Services;

namespace Crow.ViewModels;

public sealed class ThemeSettingsViewModel : BaseViewModel
{
    readonly ThemeService _themeService;

    int _selectedThemeIndex;

    public ThemeSettingsViewModel(ThemeService themeService)
    {
        _themeService = themeService;
        _selectedThemeIndex = (int)_themeService.GetPreference();
    }

    public IReadOnlyList<string> ThemeOptions { get; } =
    [
        "Use system setting",
        "Light",
        "Dark",
    ];

    public int SelectedThemeIndex
    {
        get => _selectedThemeIndex;
        set
        {
            if (_selectedThemeIndex == value)
                return;
            _selectedThemeIndex = value;
            OnPropertyChanged();
            if (value >= 0 && value <= 2)
                _themeService.SetPreference((ThemePreference)value);
        }
    }
}
