using System.ComponentModel.DataAnnotations;

namespace Dadstart.Labs.Crow.Models.Dtos;

public record RegisterDto(
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    string Username,
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    string Email,
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
    string Password);

