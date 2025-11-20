using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.Services;

public interface IApiService
{
    // Notes
    Task<List<Note>> GetNotesAsync();
    Task<Note?> GetNoteAsync(Guid id);
    Task<Note> CreateNoteAsync(CreateNoteDto dto);
    Task<Note?> UpdateNoteAsync(Guid id, UpdateNoteDto dto);
    Task<bool> DeleteNoteAsync(Guid id);

    // Passwords
    Task<List<PasswordResponseDto>> GetPasswordsAsync();
    Task<PasswordResponseDto?> GetPasswordAsync(Guid id);
    Task<PasswordResponseDto> CreatePasswordAsync(CreatePasswordDto dto);
    Task<PasswordResponseDto?> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto);
    Task<bool> DeletePasswordAsync(Guid id);

    // Reminders
    Task<List<Reminder>> GetRemindersAsync();
    Task<Reminder?> GetReminderAsync(Guid id);
    Task<Reminder> CreateReminderAsync(CreateReminderDto dto);
    Task<Reminder?> UpdateReminderAsync(Guid id, UpdateReminderDto dto);
    Task<bool> DeleteReminderAsync(Guid id);
}

