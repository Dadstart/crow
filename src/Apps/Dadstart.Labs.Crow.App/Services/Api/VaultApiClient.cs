namespace Dadstart.Labs.Crow.Services.Api;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Constants;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Security;

public sealed class VaultApiClient : IVaultApiClient
{
    static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    readonly HttpClient _httpClient;

    public VaultApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task DeleteNoteAsync(string sessionToken, Guid noteId, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, $"{VaultApiRoutes.Notes}/{noteId}", null, sessionToken, cancellationToken);

    public Task DeletePasswordAsync(string sessionToken, Guid passwordId, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, $"{VaultApiRoutes.Passwords}/{passwordId}", null, sessionToken, cancellationToken);

    public Task DeleteReminderAsync(string sessionToken, Guid reminderId, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Delete, $"{VaultApiRoutes.Reminders}/{reminderId}", null, sessionToken, cancellationToken);

    public Task<IReadOnlyList<SecureNote>> GetNotesAsync(string sessionToken, string? query, CancellationToken cancellationToken = default)
        => SendAsync<IReadOnlyList<SecureNote>>(HttpMethod.Get, BuildQuery(VaultApiRoutes.Notes, query), null, sessionToken, cancellationToken);

    public Task<IReadOnlyList<PasswordEntry>> GetPasswordsAsync(string sessionToken, string? query, CancellationToken cancellationToken = default)
        => SendAsync<IReadOnlyList<PasswordEntry>>(HttpMethod.Get, BuildQuery(VaultApiRoutes.Passwords, query), null, sessionToken, cancellationToken);

    public Task<IReadOnlyList<ReminderEntry>> GetRemindersAsync(string sessionToken, CancellationToken cancellationToken = default)
        => SendAsync<IReadOnlyList<ReminderEntry>>(HttpMethod.Get, VaultApiRoutes.Reminders, null, sessionToken, cancellationToken);

    public Task RegisterBiometricAsync(string sessionToken, string deviceId, string biometricToken, CancellationToken cancellationToken = default)
    {
        var payload = new BiometricRegistrationRequest
        {
            DeviceId = deviceId,
            BiometricToken = biometricToken
        };

        return SendAsync(HttpMethod.Post, VaultApiRoutes.Biometric, payload, sessionToken, cancellationToken);
    }

    public Task<VaultSetupState> ConfigureAsync(VaultSetupRequest request, CancellationToken cancellationToken = default)
        => SendAsync<VaultSetupState>(HttpMethod.Post, VaultApiRoutes.Setup, request, null, cancellationToken);

    public Task<VaultSetupState> GetSetupStateAsync(CancellationToken cancellationToken = default)
        => SendAsync<VaultSetupState>(HttpMethod.Get, VaultApiRoutes.SetupState, null, null, cancellationToken);

    public Task<SecureNote> UpsertNoteAsync(string sessionToken, NoteMutation mutation, CancellationToken cancellationToken = default)
        => SendAsync<SecureNote>(HttpMethod.Post, VaultApiRoutes.Notes, mutation, sessionToken, cancellationToken);

    public Task<PasswordEntry> UpsertPasswordAsync(string sessionToken, PasswordMutation mutation, CancellationToken cancellationToken = default)
        => SendAsync<PasswordEntry>(HttpMethod.Post, VaultApiRoutes.Passwords, mutation, sessionToken, cancellationToken);

    public Task<ReminderEntry> UpsertReminderAsync(string sessionToken, ReminderMutation mutation, CancellationToken cancellationToken = default)
        => SendAsync<ReminderEntry>(HttpMethod.Post, VaultApiRoutes.Reminders, mutation, sessionToken, cancellationToken);

    public Task<UnlockResponse> UnlockAsync(UnlockRequest request, CancellationToken cancellationToken = default)
        => SendAsync<UnlockResponse>(HttpMethod.Post, VaultApiRoutes.Unlock, request, null, cancellationToken);

    Task SendAsync(HttpMethod method, string path, object? payload, string? sessionToken, CancellationToken cancellationToken)
        => SendAsync<object?>(method, path, payload, sessionToken, cancellationToken);

    async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string path, object? payload, string? sessionToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: SerializerOptions);
        }

        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            request.Headers.TryAddWithoutValidation(VaultApiRoutes.SessionHeader, sessionToken);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (typeof(TResponse) == typeof(object))
        {
            return default!;
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(SerializerOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Server returned an empty payload.");
    }

    static string BuildQuery(string path, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return path;
        }

        var encoded = Uri.EscapeDataString(query);
        var builder = new StringBuilder(path);
        builder.Append("?query=").Append(encoded);
        return builder.ToString();
    }
}

