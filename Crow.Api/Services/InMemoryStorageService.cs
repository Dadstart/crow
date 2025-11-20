using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.Api.Services;

public class InMemoryStorageService : IStorageService
{
    private readonly Dictionary<Guid, Note> _notes = [];
    private readonly Dictionary<Guid, Password> _passwords = [];
    private readonly Dictionary<Guid, Reminder> _reminders = [];
    private readonly Dictionary<Guid, User> _users = [];
    private readonly Dictionary<string, Guid> _usernameIndex = [];
    private readonly Dictionary<string, Guid> _emailIndex = [];
    private readonly Dictionary<string, (Guid UserId, DateTime ExpiresAt)> _refreshTokens = [];

    // Notes
    public Task<List<Note>> GetAllNotesAsync()
    {
        return Task.FromResult(_notes.Values.ToList());
    }

    public Task<Note?> GetNoteByIdAsync(Guid id)
    {
        _notes.TryGetValue(id, out var note);
        return Task.FromResult(note);
    }

    public Task<Note> CreateNoteAsync(Note note)
    {
        _notes[note.Id] = note;
        return Task.FromResult(note);
    }

    public Task<Note?> UpdateNoteAsync(Guid id, Note note)
    {
        if (!_notes.ContainsKey(id))
            return Task.FromResult<Note?>(null);

        _notes[id] = note;
        return Task.FromResult<Note?>(note);
    }

    public Task<bool> DeleteNoteAsync(Guid id)
    {
        return Task.FromResult(_notes.Remove(id));
    }

    // Passwords
    public Task<List<Password>> GetAllPasswordsAsync()
    {
        return Task.FromResult(_passwords.Values.ToList());
    }

    public Task<Password?> GetPasswordByIdAsync(Guid id)
    {
        _passwords.TryGetValue(id, out var password);
        return Task.FromResult(password);
    }

    public Task<Password> CreatePasswordAsync(Password password)
    {
        _passwords[password.Id] = password;
        return Task.FromResult(password);
    }

    public Task<Password?> UpdatePasswordAsync(Guid id, Password password)
    {
        if (!_passwords.ContainsKey(id))
            return Task.FromResult<Password?>(null);

        _passwords[id] = password;
        return Task.FromResult<Password?>(password);
    }

    public Task<bool> DeletePasswordAsync(Guid id)
    {
        return Task.FromResult(_passwords.Remove(id));
    }

    // Reminders
    public Task<List<Reminder>> GetAllRemindersAsync()
    {
        return Task.FromResult(_reminders.Values.ToList());
    }

    public Task<Reminder?> GetReminderByIdAsync(Guid id)
    {
        _reminders.TryGetValue(id, out var reminder);
        return Task.FromResult(reminder);
    }

    public Task<Reminder> CreateReminderAsync(Reminder reminder)
    {
        _reminders[reminder.Id] = reminder;
        return Task.FromResult(reminder);
    }

    public Task<Reminder?> UpdateReminderAsync(Guid id, Reminder reminder)
    {
        if (!_reminders.ContainsKey(id))
            return Task.FromResult<Reminder?>(null);

        _reminders[id] = reminder;
        return Task.FromResult<Reminder?>(reminder);
    }

    public Task<bool> DeleteReminderAsync(Guid id)
    {
        return Task.FromResult(_reminders.Remove(id));
    }

    // Users
    public Task<User?> GetUserByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        if (!_usernameIndex.TryGetValue(username.ToLowerInvariant(), out var userId))
            return Task.FromResult<User?>(null);

        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        if (!_emailIndex.TryGetValue(email.ToLowerInvariant(), out var userId))
            return Task.FromResult<User?>(null);

        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        _users[user.Id] = user;
        _usernameIndex[user.Username.ToLowerInvariant()] = user.Id;
        _emailIndex[user.Email.ToLowerInvariant()] = user.Id;
        return Task.FromResult(user);
    }

    // Refresh Tokens
    public Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt)
    {
        _refreshTokens[refreshToken] = (userId, expiresAt);
        return Task.CompletedTask;
    }

    public Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var tokenData))
            return Task.FromResult<Guid?>(null);

        if (tokenData.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.Remove(refreshToken);
            return Task.FromResult<Guid?>(null);
        }

        return Task.FromResult<Guid?>(tokenData.UserId);
    }

    public Task RevokeRefreshTokenAsync(string refreshToken)
    {
        _refreshTokens.Remove(refreshToken);
        return Task.CompletedTask;
    }

    public Task RevokeAllRefreshTokensForUserAsync(Guid userId)
    {
        var tokensToRemove = _refreshTokens
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in tokensToRemove)
            _refreshTokens.Remove(token);

        return Task.CompletedTask;
    }
}

