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
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (string.IsNullOrWhiteSpace(encryptedPassword))
            throw new ArgumentException("Encrypted password cannot be null or empty", nameof(encryptedPassword));

        var now = _timeProvider.GetUtcNow();
        return new Password(
            Id: Guid.NewGuid(),
            Title: title.Trim(),
            Username: username.Trim(),
            EncryptedPassword: encryptedPassword,
            Url: url?.Trim(),
            Notes: notes?.Trim(),
            CreatedAt: now,
            UpdatedAt: now);
    }

    public Password WithUpdate(Password password, string? title = null, string? username = null, string? encryptedPassword = null, string? url = null, string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (title != null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (username != null && string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (encryptedPassword != null && string.IsNullOrWhiteSpace(encryptedPassword))
            throw new ArgumentException("Encrypted password cannot be empty", nameof(encryptedPassword));

        return password with
        {
            Title = title?.Trim() ?? password.Title,
            Username = username?.Trim() ?? password.Username,
            EncryptedPassword = encryptedPassword ?? password.EncryptedPassword,
            Url = url?.Trim() ?? password.Url,
            Notes = notes?.Trim() ?? password.Notes,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }
}

