using System.ComponentModel.DataAnnotations;

namespace Dadstart.Labs.Crow.Models.Dtos;

public record UpdatePasswordDto(
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    string? Title,
    
    [MaxLength(255, ErrorMessage = "Username cannot exceed 255 characters")]
    string? Username,
    
    [MaxLength(500, ErrorMessage = "Password cannot exceed 500 characters")]
    string? Password,
    
    [MaxLength(2048, ErrorMessage = "URL cannot exceed 2048 characters")]
    [Url(ErrorMessage = "Invalid URL format")]
    string? Url,
    
    [MaxLength(5000, ErrorMessage = "Notes cannot exceed 5000 characters")]
    string? Notes);

