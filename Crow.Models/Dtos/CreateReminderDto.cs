namespace Dadstart.Labs.Crow.Models.Dtos;

public record CreateReminderDto(string Title, string? Description, DateTime? DueDate);

