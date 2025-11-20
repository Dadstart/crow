namespace Dadstart.Labs.Crow.Models;

public record Reminder(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

