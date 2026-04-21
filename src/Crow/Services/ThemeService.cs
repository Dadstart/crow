using Crow.Models;
using Microsoft.Maui.Storage;

namespace Crow.Services;

public sealed class ThemeService
{
    public const string PreferenceKey = "crow_app_theme";

    public ThemePreference GetPreference()
    {
        var stored = Preferences.Default.Get(PreferenceKey, "system");
        return ParseStored(stored);
    }

    public void SetPreference(ThemePreference preference)
    {
        Preferences.Default.Set(PreferenceKey, ToStorageValue(preference));
        if (Application.Current is { } app)
            app.UserAppTheme = ToAppTheme(preference);
    }

    public static AppTheme GetAppThemeForStartup()
    {
        var stored = Preferences.Default.Get(PreferenceKey, "system");
        return ToAppTheme(ParseStored(stored));
    }

    static ThemePreference ParseStored(string stored) => stored switch
    {
        "light" => ThemePreference.Light,
        "dark" => ThemePreference.Dark,
        _ => ThemePreference.System,
    };

    static string ToStorageValue(ThemePreference preference) => preference switch
    {
        ThemePreference.Light => "light",
        ThemePreference.Dark => "dark",
        _ => "system",
    };

    static AppTheme ToAppTheme(ThemePreference preference) => preference switch
    {
        ThemePreference.Light => AppTheme.Light,
        ThemePreference.Dark => AppTheme.Dark,
        _ => AppTheme.Unspecified,
    };
}
