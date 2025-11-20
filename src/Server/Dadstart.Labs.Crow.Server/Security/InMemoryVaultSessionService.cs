namespace Dadstart.Labs.Crow.Server.Security;

using System.Collections.Concurrent;
using System.Security.Cryptography;

internal sealed class InMemoryVaultSessionService : IVaultSessionService
{
    readonly ConcurrentDictionary<string, VaultSession> _sessions = new(StringComparer.Ordinal);

    public Task<VaultSession?> GetAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<VaultSession?>(null);
        }

        if (!_sessions.TryGetValue(token, out var session))
        {
            return Task.FromResult<VaultSession?>(null);
        }

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(token, out _);
            return Task.FromResult<VaultSession?>(null);
        }

        return Task.FromResult<VaultSession?>(session);
    }

    public Task<VaultSession> IssueAsync(string deviceId, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var session = new VaultSession
        {
            Token = token,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime),
            DeviceId = deviceId
        };

        _sessions[token] = session;
        return Task.FromResult(session);
    }

    public Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _sessions.TryRemove(token, out _);
        }

        return Task.CompletedTask;
    }
}

