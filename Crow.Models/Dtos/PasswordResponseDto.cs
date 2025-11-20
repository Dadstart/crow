namespace Dadstart.Labs.Crow.Models.Dtos;

public record PasswordResponseDto(
    Guid Id,
    string Title,
    string Username,
    string? DecryptedPassword,
    string? Url,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

