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
        var now = _timeProvider.GetUtcNow();
        return new Reminder(
            Id: Guid.NewGuid(),
            Title: title,
            Description: description,
            DueDate: dueDate,
            IsCompleted: false,
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Reminder WithUpdate(Reminder reminder, string? title = null, string? description = null, DateTimeOffset? dueDate = null, bool? isCompleted = null)
    {
        return reminder with
        {
            Title = title ?? reminder.Title,
            Description = description ?? reminder.Description,
            DueDate = dueDate ?? reminder.DueDate,
            IsCompleted = isCompleted ?? reminder.IsCompleted,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }

    public Reminder MarkCompleted(Reminder reminder)
    {
        return reminder with { IsCompleted = true, UpdatedAt = _timeProvider.GetUtcNow() };
    }

    public Reminder MarkIncomplete(Reminder reminder)
    {
        return reminder with { IsCompleted = false, UpdatedAt = _timeProvider.GetUtcNow() };
    }
}

