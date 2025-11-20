namespace Dadstart.Labs.Crow.Models;

/// <summary>
/// Time-based reminder that can surface as a local notification.
/// </summary>
public sealed record class ReminderEntry : VaultItem
{
    public string Body { get; init; } = string.Empty;

    public DateTimeOffset ScheduledAt { get; init; } = DateTimeOffset.UtcNow.AddMinutes(5);

    public ReminderRecurrence Recurrence { get; init; }

    public TimeSpan? SnoozeDuration { get; init; }

    public bool IsCompleted { get; init; }
}

