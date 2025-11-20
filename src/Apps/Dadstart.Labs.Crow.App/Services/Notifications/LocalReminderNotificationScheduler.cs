namespace Dadstart.Labs.Crow.Services.Notifications;

using System.Collections.Concurrent;
using System.Linq;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Notifications;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

public sealed class LocalReminderNotificationScheduler : IReminderNotificationScheduler
{
    readonly ConcurrentDictionary<Guid, CancellationTokenSource> _pending = new();

    public Task CancelAsync(Guid reminderId, CancellationToken cancellationToken)
    {
        if (_pending.TryRemove(reminderId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        return Task.CompletedTask;
    }

    public Task ScheduleAsync(ReminderEntry reminder, CancellationToken cancellationToken)
    {
        CancelAsync(reminder.Id, CancellationToken.None);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _pending[reminder.Id] = cts;
        _ = RunReminderAsync(reminder, cts.Token);

        return Task.CompletedTask;
    }

    async Task RunReminderAsync(ReminderEntry reminder, CancellationToken cancellationToken)
    {
        var delay = reminder.ScheduledAt - DateTimeOffset.UtcNow;
        if (delay > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlert("Reminder", $"{reminder.Title}\n\n{reminder.Body}", "Dismiss");
            }

            if (Vibration.Default.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
            }
        });

        if (_pending.TryRemove(reminder.Id, out var cts))
        {
            cts.Dispose();
        }
    }
}

