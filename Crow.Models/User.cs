namespace Dadstart.Labs.Crow.Models;

public record User(
    Guid Id,
    string Username,
    string Email,
    string PasswordHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static User Create(string username, string email, string passwordHash)
    {
        var now = DateTimeOffset.UtcNow;
        return new User(
            Id: Guid.NewGuid(),
            Username: username,
            Email: email,
            PasswordHash: passwordHash,
            CreatedAt: now,
            UpdatedAt: now);
    }
}

