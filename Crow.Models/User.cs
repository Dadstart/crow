namespace Dadstart.Labs.Crow.Models;

public record User(
    Guid Id,
    string Username,
    string Email,
    string PasswordHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

