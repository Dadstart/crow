using Crow.Models;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
#endif
#if IOS || MACCATALYST
using Foundation;
using UserNotifications;
#endif

namespace Crow.Services;

public sealed class TaskNotificationService : ITaskNotificationService
{
#if ANDROID
    const string NotificationChannelId = "task-reminders";
    const string NotificationChannelName = "Task reminders";
#endif

    public async Task EnsurePermissionsAsync()
    {
#if ANDROID
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            await Permissions.RequestAsync<Permissions.PostNotifications>().ConfigureAwait(false);
#elif IOS || MACCATALYST
        _ = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge)
            .ConfigureAwait(false);
#else
        await Task.CompletedTask.ConfigureAwait(false);
#endif
    }

    public async Task ScheduleTaskReminderAsync(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        await CancelTaskReminderAsync(task.Id).ConfigureAwait(false);

        if (task.IsCompleted || !task.DueDate.HasValue)
            return;

        var fireAtLocal = BuildReminderTime(task.DueDate.Value);
        if (fireAtLocal <= DateTime.Now)
            return;

        await EnsurePermissionsAsync().ConfigureAwait(false);

#if ANDROID
        EnsureAndroidChannel();

        var context = Platform.AppContext;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
            return;

        var intent = new Intent(context, typeof(TaskReminderReceiver));
        intent.SetAction(TaskReminderReceiver.ActionName);
        intent.PutExtra(TaskReminderReceiver.ExtraTaskId, task.Id.ToString());
        intent.PutExtra(TaskReminderReceiver.ExtraTitle, task.Title);
        intent.PutExtra(TaskReminderReceiver.ExtraDescription, task.Description);

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            GetRequestCode(task.Id),
            intent,
            PendingIntentFlags.UpdateCurrent | GetImmutableFlag());
        if (pendingIntent is null)
            return;

        var triggerAtMillis = new DateTimeOffset(fireAtLocal).ToUnixTimeMilliseconds();
        alarmManager.Set(AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
#elif IOS || MACCATALYST
        var content = new UNMutableNotificationContent
        {
            Title = string.IsNullOrWhiteSpace(task.Title) ? "Task reminder" : task.Title,
            Body = string.IsNullOrWhiteSpace(task.Description) ? "A task is due today." : task.Description,
            Sound = UNNotificationSound.Default,
        };

        var components = new NSDateComponents
        {
            Year = fireAtLocal.Year,
            Month = fireAtLocal.Month,
            Day = fireAtLocal.Day,
            Hour = fireAtLocal.Hour,
            Minute = fireAtLocal.Minute,
        };
        var trigger = UNCalendarNotificationTrigger.CreateTrigger(components, false);
        var request = UNNotificationRequest.FromIdentifier(task.Id.ToString(), content, trigger);

        await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request).ConfigureAwait(false);
#else
        await Task.CompletedTask.ConfigureAwait(false);
#endif
    }

    public async Task CancelTaskReminderAsync(Guid taskId)
    {
        if (taskId == Guid.Empty)
            return;

#if ANDROID
        var context = Platform.AppContext;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
            return;

        var intent = new Intent(context, typeof(TaskReminderReceiver));
        intent.SetAction(TaskReminderReceiver.ActionName);
        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            GetRequestCode(taskId),
            intent,
            PendingIntentFlags.UpdateCurrent | GetImmutableFlag());
        if (pendingIntent is null)
            return;
        alarmManager.Cancel(pendingIntent);
        pendingIntent.Cancel();
#elif IOS || MACCATALYST
        var identifiers = new[] { taskId.ToString() };
        UNUserNotificationCenter.Current.RemovePendingNotificationRequests(identifiers);
        UNUserNotificationCenter.Current.RemoveDeliveredNotifications(identifiers);
#else
        await Task.CompletedTask.ConfigureAwait(false);
#endif

        await Task.CompletedTask.ConfigureAwait(false);
    }

    static DateTime BuildReminderTime(DateTime dueDateUtc)
    {
        var local = DateTime.SpecifyKind(dueDateUtc, DateTimeKind.Utc).ToLocalTime();
        return new DateTime(local.Year, local.Month, local.Day, 9, 0, 0, DateTimeKind.Local);
    }

#if ANDROID
    static int GetRequestCode(Guid id) => id.GetHashCode();

    static PendingIntentFlags GetImmutableFlag() =>
        Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.Immutable : 0;

    static void EnsureAndroidChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var context = Platform.AppContext;
        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null)
            return;

        if (manager.GetNotificationChannel(NotificationChannelId) is not null)
            return;

        var channel = new NotificationChannel(NotificationChannelId, NotificationChannelName, (NotificationImportance)3)
        {
            Description = "Due-date reminders for tasks",
        };
        manager.CreateNotificationChannel(channel);
    }
#endif
}
