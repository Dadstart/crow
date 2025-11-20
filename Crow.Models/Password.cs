namespace Dadstart.Labs.Crow.Models;

public record Password(
    Guid Id,
    string Title,
    string Username,
    string EncryptedPassword,
    string? Url,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static Password Create(string title, string username, string encryptedPassword, string? url = null, string? notes = null)
    {
        var now = DateTime.UtcNow;
        return new Password(
            Id: Guid.NewGuid(),
            Title: title,
            Username: username,
            EncryptedPassword: encryptedPassword,
            Url: url,
            Notes: notes,
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Password WithUpdate(string? title = null, string? username = null, string? encryptedPassword = null, string? url = null, string? notes = null)
    {
        return this with
        {
            Title = title ?? Title,
            Username = username ?? Username,
            EncryptedPassword = encryptedPassword ?? EncryptedPassword,
            Url = url ?? Url,
            Notes = notes ?? Notes,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

