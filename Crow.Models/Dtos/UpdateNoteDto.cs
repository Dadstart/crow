using System.ComponentModel.DataAnnotations;

namespace Dadstart.Labs.Crow.Models.Dtos;

public record UpdateNoteDto(
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    string? Title,
    
    [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    string? Content,
    
    List<string>? Tags);

