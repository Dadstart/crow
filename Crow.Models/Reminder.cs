namespace Dadstart.Labs.Crow.Models;

public record Reminder(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static Reminder Create(string title, string? description = null, DateTime? dueDate = null)
    {
        var now = DateTime.UtcNow;
        return new Reminder(
            Id: Guid.NewGuid(),
            Title: title,
            Description: description,
            DueDate: dueDate,
            IsCompleted: false,
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Reminder WithUpdate(string? title = null, string? description = null, DateTime? dueDate = null, bool? isCompleted = null)
    {
        return this with
        {
            Title = title ?? Title,
            Description = description ?? Description,
            DueDate = dueDate ?? DueDate,
            IsCompleted = isCompleted ?? IsCompleted,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Reminder MarkCompleted()
    {
        return this with { IsCompleted = true, UpdatedAt = DateTime.UtcNow };
    }

    public Reminder MarkIncomplete()
    {
        return this with { IsCompleted = false, UpdatedAt = DateTime.UtcNow };
    }
}

