using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.Api.Services;

public interface IStorageService
{
    // Notes
    Task<List<Note>> GetAllNotesAsync();
    Task<Note?> GetNoteByIdAsync(Guid id);
    Task<Note> CreateNoteAsync(Note note);
    Task<Note?> UpdateNoteAsync(Guid id, Note note);
    Task<bool> DeleteNoteAsync(Guid id);

    // Passwords
    Task<List<Password>> GetAllPasswordsAsync();
    Task<Password?> GetPasswordByIdAsync(Guid id);
    Task<Password> CreatePasswordAsync(Password password);
    Task<Password?> UpdatePasswordAsync(Guid id, Password password);
    Task<bool> DeletePasswordAsync(Guid id);

    // Reminders
    Task<List<Reminder>> GetAllRemindersAsync();
    Task<Reminder?> GetReminderByIdAsync(Guid id);
    Task<Reminder> CreateReminderAsync(Reminder reminder);
    Task<Reminder?> UpdateReminderAsync(Guid id, Reminder reminder);
    Task<bool> DeleteReminderAsync(Guid id);

    // Users
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
}

