namespace Dadstart.Labs.Crow.Services.Security;

using Dadstart.Labs.Crow.Security;

public sealed class VaultSessionState
{
    readonly object _gate = new();

    public event EventHandler? SessionRevoked;

    public bool IsAuthenticated => Token is not null;

    public string? Token { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public void StartSession(UnlockResponse response)
    {
        lock (_gate)
        {
            Token = response.SessionToken;
            ExpiresAt = response.ExpiresAt;
        }
    }

    public string RequireToken()
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(Token))
        {
            throw new InvalidOperationException("Vault session missing.");
        }

        return Token!;
    }

    public void Revoke()
    {
        lock (_gate)
        {
            Token = null;
            ExpiresAt = null;
        }

        SessionRevoked?.Invoke(this, EventArgs.Empty);
    }
}

