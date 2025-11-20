namespace Dadstart.Labs.Crow.Models.Factories;

public class ReminderFactory
{
    private readonly TimeProvider _timeProvider;

    public ReminderFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Reminder Create(string title, string? description = null, DateTimeOffset? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        var now = _timeProvider.GetUtcNow();
        return new Reminder(
            Id: Guid.NewGuid(),
            Title: title.Trim(),
            Description: description?.Trim(),
            DueDate: dueDate,
            IsCompleted: false,
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Reminder WithUpdate(Reminder reminder, string? title = null, string? description = null, DateTimeOffset? dueDate = null, bool? isCompleted = null)
    {
        ArgumentNullException.ThrowIfNull(reminder);

        if (title != null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        return reminder with
        {
            Title = title?.Trim() ?? reminder.Title,
            Description = description?.Trim() ?? reminder.Description,
            DueDate = dueDate ?? reminder.DueDate,
            IsCompleted = isCompleted ?? reminder.IsCompleted,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }

    public Reminder MarkCompleted(Reminder reminder)
    {
        ArgumentNullException.ThrowIfNull(reminder);
        return reminder with { IsCompleted = true, UpdatedAt = _timeProvider.GetUtcNow() };
    }

    public Reminder MarkIncomplete(Reminder reminder)
    {
        ArgumentNullException.ThrowIfNull(reminder);
        return reminder with { IsCompleted = false, UpdatedAt = _timeProvider.GetUtcNow() };
    }
}

