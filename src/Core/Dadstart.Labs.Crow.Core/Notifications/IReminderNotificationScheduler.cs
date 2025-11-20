namespace Dadstart.Labs.Crow.Notifications;

using Dadstart.Labs.Crow.Models;

public interface IReminderNotificationScheduler
{
    Task ScheduleAsync(ReminderEntry reminder, CancellationToken cancellationToken);

    Task CancelAsync(Guid reminderId, CancellationToken cancellationToken);
}

