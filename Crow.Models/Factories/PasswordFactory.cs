namespace Dadstart.Labs.Crow.Models.Factories;

public class PasswordFactory
{
    private readonly TimeProvider _timeProvider;

    public PasswordFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Password Create(string title, string username, string encryptedPassword, string? url = null, string? notes = null)
    {
        var now = _timeProvider.GetUtcNow();
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

    public Password WithUpdate(Password password, string? title = null, string? username = null, string? encryptedPassword = null, string? url = null, string? notes = null)
    {
        return password with
        {
            Title = title ?? password.Title,
            Username = username ?? password.Username,
            EncryptedPassword = encryptedPassword ?? password.EncryptedPassword,
            Url = url ?? password.Url,
            Notes = notes ?? password.Notes,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }
}

