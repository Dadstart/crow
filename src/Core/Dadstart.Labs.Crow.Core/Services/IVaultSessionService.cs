namespace Dadstart.Labs.Crow.Services;

using Dadstart.Labs.Crow.Security;

public interface IVaultSessionService
{
    Task<VaultSession> IssueAsync(string deviceId, TimeSpan lifetime, CancellationToken cancellationToken);

    Task<VaultSession?> GetAsync(string token, CancellationToken cancellationToken);

    Task RevokeAsync(string token, CancellationToken cancellationToken);
}

