namespace Dadstart.Labs.Crow.Models.Dtos;

public record UpdateReminderDto(string? Title, string? Description, DateTimeOffset? DueDate, bool? IsCompleted);

