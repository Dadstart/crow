namespace Dadstart.Labs.Crow.Services;

using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;

public interface IVaultRepository
{
    Task<IReadOnlyList<SecureNote>> GetNotesAsync(string? query, CancellationToken cancellationToken);

    Task<SecureNote?> GetNoteAsync(Guid id, CancellationToken cancellationToken);

    Task<SecureNote> UpsertNoteAsync(NoteMutation mutation, CancellationToken cancellationToken);

    Task DeleteNoteAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PasswordEntry>> GetPasswordsAsync(string? query, CancellationToken cancellationToken);

    Task<PasswordEntry?> GetPasswordAsync(Guid id, CancellationToken cancellationToken);

    Task<PasswordEntry> UpsertPasswordAsync(PasswordMutation mutation, CancellationToken cancellationToken);

    Task DeletePasswordAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReminderEntry>> GetRemindersAsync(CancellationToken cancellationToken);

    Task<ReminderEntry?> GetReminderAsync(Guid id, CancellationToken cancellationToken);

    Task<ReminderEntry> UpsertReminderAsync(ReminderMutation mutation, CancellationToken cancellationToken);

    Task DeleteReminderAsync(Guid id, CancellationToken cancellationToken);
}

