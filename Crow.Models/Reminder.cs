namespace Dadstart.Labs.Crow.Models;

public record Reminder(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static Reminder Create(string title, string? description = null, DateTimeOffset? dueDate = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new Reminder(
            Id: Guid.NewGuid(),
            Title: title,
            Description: description,
            DueDate: dueDate,
            IsCompleted: false,
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Reminder WithUpdate(string? title = null, string? description = null, DateTimeOffset? dueDate = null, bool? isCompleted = null)
    {
        return this with
        {
            Title = title ?? Title,
            Description = description ?? Description,
            DueDate = dueDate ?? DueDate,
            IsCompleted = isCompleted ?? IsCompleted,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public Reminder MarkCompleted()
    {
        return this with { IsCompleted = true, UpdatedAt = DateTimeOffset.UtcNow };
    }

    public Reminder MarkIncomplete()
    {
        return this with { IsCompleted = false, UpdatedAt = DateTimeOffset.UtcNow };
    }
}

