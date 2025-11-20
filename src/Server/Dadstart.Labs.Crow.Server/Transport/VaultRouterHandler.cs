namespace Dadstart.Labs.Crow.Server.Transport;

using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dadstart.Labs.Crow.Server.Security;

internal sealed class VaultRouterHandler : HttpMessageHandler
{
    readonly IVaultSecurityService _securityService;
    readonly IVaultRepository _repository;
    readonly IVaultSessionService _sessionService;
    readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public VaultRouterHandler(
        IVaultSecurityService securityService,
        IVaultRepository repository,
        IVaultSessionService sessionService)
    {
        _securityService = securityService;
        _repository = repository;
        _sessionService = sessionService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = NormalizePath(request.RequestUri?.AbsolutePath);

        if (request.Method == HttpMethod.Get && path.Equals(VaultApiRoutes.SetupState, StringComparison.OrdinalIgnoreCase))
        {
            var state = await _securityService.GetSetupStateAsync(cancellationToken);
            return CreateJsonResponse(state);
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Setup, StringComparison.OrdinalIgnoreCase))
        {
            var payload = await ReadJsonAsync<VaultSetupRequest>(request, cancellationToken);
            if (payload is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid setup payload.");
            }

            try
            {
                var state = await _securityService.ConfigureAsync(payload, cancellationToken);
                return CreateJsonResponse(state);
            }
            catch (InvalidOperationException ex)
            {
                return CreateError(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Unlock, StringComparison.OrdinalIgnoreCase))
        {
            var payload = await ReadJsonAsync<UnlockRequest>(request, cancellationToken);
            if (payload is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid unlock payload.");
            }

            try
            {
                var response = await _securityService.UnlockAsync(payload, cancellationToken);
                return CreateJsonResponse(response);
            }
            catch (UnauthorizedAccessException)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Biometric, StringComparison.OrdinalIgnoreCase))
        {
            var token = await ValidateSessionAsync(request, cancellationToken);
            if (token is null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var payload = await ReadJsonAsync<BiometricRegistrationRequest>(request, cancellationToken);
            if (payload is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid biometric payload.");
            }

            await _securityService.RegisterBiometricAsync(token, payload.DeviceId, payload.BiometricToken, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        if (path.StartsWith(VaultApiRoutes.Notes, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleNotesAsync(request, path, cancellationToken);
        }

        if (path.StartsWith(VaultApiRoutes.Passwords, StringComparison.OrdinalIgnoreCase))
        {
            return await HandlePasswordsAsync(request, path, cancellationToken);
        }

        if (path.StartsWith(VaultApiRoutes.Reminders, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleRemindersAsync(request, path, cancellationToken);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    async Task<HttpResponseMessage> HandleNotesAsync(HttpRequestMessage request, string path, CancellationToken cancellationToken)
    {
        var token = await ValidateSessionAsync(request, cancellationToken);
        if (token is null)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        if (request.Method == HttpMethod.Get && path.Equals(VaultApiRoutes.Notes, StringComparison.OrdinalIgnoreCase))
        {
            var query = GetQueryValue(request.RequestUri, "query");
            var notes = await _repository.GetNotesAsync(query, cancellationToken);
            return CreateJsonResponse(notes);
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Notes, StringComparison.OrdinalIgnoreCase))
        {
            var mutation = await ReadJsonAsync<NoteMutation>(request, cancellationToken);
            if (mutation is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid note payload.");
            }

            var note = await _repository.UpsertNoteAsync(mutation, cancellationToken);
            return CreateJsonResponse(note);
        }

        if (request.Method == HttpMethod.Delete && TryParseResourceId(path, VaultApiRoutes.Notes, out var noteId))
        {
            var existing = await _repository.GetNoteAsync(noteId, cancellationToken);
            if (existing is null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            await _repository.DeleteNoteAsync(noteId, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    async Task<HttpResponseMessage> HandlePasswordsAsync(HttpRequestMessage request, string path, CancellationToken cancellationToken)
    {
        var token = await ValidateSessionAsync(request, cancellationToken);
        if (token is null)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        if (request.Method == HttpMethod.Get && path.Equals(VaultApiRoutes.Passwords, StringComparison.OrdinalIgnoreCase))
        {
            var query = GetQueryValue(request.RequestUri, "query");
            var passwords = await _repository.GetPasswordsAsync(query, cancellationToken);
            return CreateJsonResponse(passwords);
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Passwords, StringComparison.OrdinalIgnoreCase))
        {
            var mutation = await ReadJsonAsync<PasswordMutation>(request, cancellationToken);
            if (mutation is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid password payload.");
            }

            var entry = await _repository.UpsertPasswordAsync(mutation, cancellationToken);
            return CreateJsonResponse(entry);
        }

        if (request.Method == HttpMethod.Delete && TryParseResourceId(path, VaultApiRoutes.Passwords, out var passwordId))
        {
            var existing = await _repository.GetPasswordAsync(passwordId, cancellationToken);
            if (existing is null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            await _repository.DeletePasswordAsync(passwordId, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    async Task<HttpResponseMessage> HandleRemindersAsync(HttpRequestMessage request, string path, CancellationToken cancellationToken)
    {
        var token = await ValidateSessionAsync(request, cancellationToken);
        if (token is null)
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        if (request.Method == HttpMethod.Get && path.Equals(VaultApiRoutes.Reminders, StringComparison.OrdinalIgnoreCase))
        {
            var reminders = await _repository.GetRemindersAsync(cancellationToken);
            return CreateJsonResponse(reminders);
        }

        if (request.Method == HttpMethod.Post && path.Equals(VaultApiRoutes.Reminders, StringComparison.OrdinalIgnoreCase))
        {
            var mutation = await ReadJsonAsync<ReminderMutation>(request, cancellationToken);
            if (mutation is null)
            {
                return CreateError(HttpStatusCode.BadRequest, "Invalid reminder payload.");
            }

            var reminder = await _repository.UpsertReminderAsync(mutation, cancellationToken);
            return CreateJsonResponse(reminder);
        }

        if (request.Method == HttpMethod.Delete && TryParseResourceId(path, VaultApiRoutes.Reminders, out var reminderId))
        {
            var existing = await _repository.GetReminderAsync(reminderId, cancellationToken);
            if (existing is null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            await _repository.DeleteReminderAsync(reminderId, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    async Task<string?> ValidateSessionAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues(VaultApiRoutes.SessionHeader, out var values))
        {
            return null;
        }

        var token = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var session = await _sessionService.GetAsync(token, cancellationToken);
        return session?.Token;
    }

    async Task<T?> ReadJsonAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is null)
        {
            return default;
        }

        await using var stream = await request.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _serializerOptions, cancellationToken);
    }

    static string NormalizePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "/";
        }

        return path.Length > 1 && path.EndsWith('/') ? path.TrimEnd('/') : path;
    }

    static bool TryParseResourceId(string path, string resourceRoot, out Guid resourceId)
    {
        resourceId = Guid.Empty;
        if (!path.StartsWith(resourceRoot, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var tail = path[resourceRoot.Length..].Trim('/');
        return Guid.TryParse(tail, out resourceId);
    }

    static string? GetQueryValue(Uri? uri, string key)
    {
        if (uri is null || string.IsNullOrEmpty(uri.Query))
        {
            return null;
        }

        var query = uri.Query.TrimStart('?');
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }

    HttpResponseMessage CreateJsonResponse<T>(T payload, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, _serializerOptions), Encoding.UTF8, "application/json")
        };

        return response;
    }

    HttpResponseMessage CreateError(HttpStatusCode statusCode, string message)
    {
        var payload = new { error = message };
        return CreateJsonResponse(payload, statusCode);
    }
}

