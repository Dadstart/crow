using Crow.Models;

namespace Crow.Services;

public interface ITaskNotificationService
{
    Task EnsurePermissionsAsync();

    Task ScheduleTaskReminderAsync(TaskItem task);

    Task CancelTaskReminderAsync(Guid taskId);
}
