namespace Dadstart.Labs.Crow.Models;

using System.Collections.Immutable;

/// <summary>
/// Base record for note, password, and reminder payloads stored in the vault.
/// </summary>
public abstract record class VaultItem
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; init; } = string.Empty;

    public ImmutableArray<string> Tags { get; init; } = [];

    public bool IsPinned { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

