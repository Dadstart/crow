using System.ComponentModel.DataAnnotations;

namespace Dadstart.Labs.Crow.Models.Dtos;

public record CreateReminderDto(
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    string Title,
    
    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    string? Description,
    
    DateTimeOffset? DueDate);

