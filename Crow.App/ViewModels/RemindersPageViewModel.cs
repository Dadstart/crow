using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Crow.App.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.ViewModels;

public partial class RemindersPageViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<Reminder> _reminders = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _dueDate;

    [ObservableProperty]
    private Reminder? _selectedReminder;

    public RemindersPageViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadRemindersAsync()
    {
        IsLoading = true;
        try
        {
            Reminders = await _apiService.GetRemindersAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreateReminderAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return;

        var dto = new CreateReminderDto(Title, string.IsNullOrWhiteSpace(Description) ? null : Description, DueDate);
        await _apiService.CreateReminderAsync(dto);

        Title = string.Empty;
        Description = string.Empty;
        DueDate = null;

        await LoadRemindersAsync();
    }

    [RelayCommand]
    public async Task UpdateReminderAsync(Reminder reminder)
    {
        if (reminder == null)
            return;

        var dto = new UpdateReminderDto(Title, string.IsNullOrWhiteSpace(Description) ? null : Description, DueDate, reminder.IsCompleted);
        await _apiService.UpdateReminderAsync(reminder.Id, dto);

        Title = string.Empty;
        Description = string.Empty;
        DueDate = null;
        SelectedReminder = null;

        await LoadRemindersAsync();
    }

    [RelayCommand]
    public async Task DeleteReminderAsync(Reminder reminder)
    {
        if (reminder == null)
            return;

        await _apiService.DeleteReminderAsync(reminder.Id);
        await LoadRemindersAsync();
    }

    [RelayCommand]
    public async Task ToggleReminderCompletionAsync(Reminder reminder)
    {
        if (reminder == null)
            return;

        var dto = new UpdateReminderDto(null, null, null, !reminder.IsCompleted);
        await _apiService.UpdateReminderAsync(reminder.Id, dto);
        await LoadRemindersAsync();
    }

    [RelayCommand]
    public void SelectReminder(Reminder reminder)
    {
        SelectedReminder = reminder;
        Title = reminder.Title;
        Description = reminder.Description ?? string.Empty;
        DueDate = reminder.DueDate;
    }

    [RelayCommand]
    public void ClearSelection()
    {
        SelectedReminder = null;
        Title = string.Empty;
        Description = string.Empty;
        DueDate = null;
    }
}

