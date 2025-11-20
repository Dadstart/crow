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
        var now = _timeProvider.GetUtcNow();
        return new User(
            Id: Guid.NewGuid(),
            Username: username,
            Email: email,
            PasswordHash: passwordHash,
            CreatedAt: now,
            UpdatedAt: now);
    }
}

