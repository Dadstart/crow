using Android.App;
using Android.Content;
using AndroidX.Core.App;

namespace Crow;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class TaskReminderReceiver : BroadcastReceiver
{
    public const string ActionName = "Crow.TaskReminder";
    public const string ExtraTaskId = "taskId";
    public const string ExtraTitle = "title";
    public const string ExtraDescription = "description";
    const string NotificationChannelId = "task-reminders";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
            return;

        var taskId = intent.GetStringExtra(ExtraTaskId) ?? Guid.NewGuid().ToString();
        var title = intent.GetStringExtra(ExtraTitle);
        var body = intent.GetStringExtra(ExtraDescription);

        var notification = new NotificationCompat.Builder(context, NotificationChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(string.IsNullOrWhiteSpace(title) ? "Task reminder" : title)
            .SetContentText(string.IsNullOrWhiteSpace(body) ? "A task is due today." : body)
            .SetPriority((int)NotificationPriority.Default)
            .SetAutoCancel(true)
            .Build();
        if (notification is null)
            return;

        var manager = NotificationManagerCompat.From(context);
        manager.Notify(taskId.GetHashCode(), notification);
    }
}
