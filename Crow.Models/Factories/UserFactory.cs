namespace Dadstart.Labs.Crow.Models.Factories;

public class UserFactory
{
    private readonly TimeProvider _timeProvider;

    public UserFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        var now = _timeProvider.GetUtcNow();
        return new User(
            Id: Guid.NewGuid(),
            Username: username.Trim(),
            Email: email.Trim().ToLowerInvariant(),
            PasswordHash: passwordHash,
            CreatedAt: now,
            UpdatedAt: now);
    }
}

