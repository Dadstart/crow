using System.ComponentModel.DataAnnotations;

namespace Dadstart.Labs.Crow.Models.Dtos;

public record CreateNoteDto(
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    string Title,
    
    [Required(ErrorMessage = "Content is required")]
    [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    string Content,
    
    List<string>? Tags);

