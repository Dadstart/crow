namespace Dadstart.Labs.Crow.Abstractions;

using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Security;

public interface IVaultApiClient
{
    Task<VaultSetupState> GetSetupStateAsync(CancellationToken cancellationToken = default);

    Task<VaultSetupState> ConfigureAsync(VaultSetupRequest request, CancellationToken cancellationToken = default);

    Task<UnlockResponse> UnlockAsync(UnlockRequest request, CancellationToken cancellationToken = default);

    Task RegisterBiometricAsync(string sessionToken, string deviceId, string biometricToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SecureNote>> GetNotesAsync(string sessionToken, string? query, CancellationToken cancellationToken = default);

    Task<SecureNote> UpsertNoteAsync(string sessionToken, NoteMutation mutation, CancellationToken cancellationToken = default);

    Task DeleteNoteAsync(string sessionToken, Guid noteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswordEntry>> GetPasswordsAsync(string sessionToken, string? query, CancellationToken cancellationToken = default);

    Task<PasswordEntry> UpsertPasswordAsync(string sessionToken, PasswordMutation mutation, CancellationToken cancellationToken = default);

    Task DeletePasswordAsync(string sessionToken, Guid passwordId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReminderEntry>> GetRemindersAsync(string sessionToken, CancellationToken cancellationToken = default);

    Task<ReminderEntry> UpsertReminderAsync(string sessionToken, ReminderMutation mutation, CancellationToken cancellationToken = default);

    Task DeleteReminderAsync(string sessionToken, Guid reminderId, CancellationToken cancellationToken = default);
}

