namespace Dadstart.Labs.Crow.Models;

public record Password(
    Guid Id,
    string Title,
    string Username,
    string EncryptedPassword,
    string? Url,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

