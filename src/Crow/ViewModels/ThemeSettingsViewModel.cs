using System.Windows.Input;
using Crow.Models;
using Crow.Services;
using Microsoft.Maui.ApplicationModel;

namespace Crow.ViewModels;

public sealed class ThemeSettingsViewModel : BaseViewModel
{
    readonly ThemeService _themeService;
    readonly DataExportService _dataExportService;

    int _selectedThemeIndex;
    bool _isExporting;
    string _exportMessage = string.Empty;

    public ThemeSettingsViewModel(ThemeService themeService, DataExportService dataExportService)
    {
        _themeService = themeService;
        _dataExportService = dataExportService;
        _selectedThemeIndex = (int)_themeService.GetPreference();

        ExportJsonCommand = new Command(
            async () => await ExportJsonAsync().ConfigureAwait(false),
            () => CanExport);
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

    public ICommand ExportJsonCommand { get; }

    public bool CanExport => !_isExporting;

    public string ExportMessage
    {
        get => _exportMessage;
        private set
        {
            if (_exportMessage == value)
                return;
            _exportMessage = value;
            OnPropertyChanged();
        }
    }

    async Task ExportJsonAsync()
    {
        if (_isExporting)
            return;

        _isExporting = true;
        OnPropertyChanged(nameof(CanExport));
        if (ExportJsonCommand is Command cmd)
            cmd.ChangeCanExecute();

        try
        {
            var path = await _dataExportService.ExportAllToJsonAsync().ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ExportMessage = $"Export saved to:{Environment.NewLine}{path}";
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ExportMessage = $"Export failed:{Environment.NewLine}{ex.Message}";
            }).ConfigureAwait(false);
        }
        finally
        {
            _isExporting = false;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                OnPropertyChanged(nameof(CanExport));
                if (ExportJsonCommand is Command c)
                    c.ChangeCanExecute();
            }).ConfigureAwait(false);
        }
    }
}
