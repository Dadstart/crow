namespace Dadstart.Labs.Crow.ViewModels;

using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Notifications;
using Dadstart.Labs.Crow.Services.Security;

public partial class RemindersViewModel : ObservableObject
{
    readonly IVaultApiClient _apiClient;
    readonly VaultSessionState _sessionState;
    readonly IReminderNotificationScheduler _scheduler;

    public RemindersViewModel(
        IVaultApiClient apiClient,
        VaultSessionState sessionState,
        IReminderNotificationScheduler scheduler)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;
        _scheduler = scheduler;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        AddReminderCommand = new AsyncRelayCommand(AddReminderAsync);
    }

    public ObservableCollection<ReminderEntry> Reminders { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand AddReminderCommand { get; }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    string? statusMessage;

    async Task RefreshAsync()
    {
        if (!_sessionState.IsAuthenticated)
        {
            StatusMessage = "Unlock required.";
            return;
        }

        try
        {
            IsRefreshing = true;
            var token = _sessionState.RequireToken();
            var results = await _apiClient.GetRemindersAsync(token);
            UpdateCollection(Reminders, results);
            foreach (var reminder in results)
            {
                await _scheduler.ScheduleAsync(reminder, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    async Task AddReminderAsync()
    {
        if (!_sessionState.IsAuthenticated)
        {
            StatusMessage = "Unlock required.";
            return;
        }

        try
        {
            var token = _sessionState.RequireToken();
            var mutation = new ReminderMutation
            {
                Title = "Reminder",
                Body = "Remember to rotate secrets.",
                ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            var reminder = await _apiClient.UpsertReminderAsync(token, mutation);
            await _scheduler.ScheduleAsync(reminder, CancellationToken.None);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    static void UpdateCollection<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}

