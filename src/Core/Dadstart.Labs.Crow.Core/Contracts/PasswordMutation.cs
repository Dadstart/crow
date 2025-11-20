namespace Dadstart.Labs.Crow.Contracts;

using Dadstart.Labs.Crow.Models;
using System.Collections.Immutable;

public sealed record class PasswordMutation
{
    public Guid? Id { get; init; }

    public required string Title { get; init; }

    public required string Username { get; init; }

    public required string Secret { get; init; }

    public Uri? ResourceUri { get; init; }

    public string Notes { get; init; } = string.Empty;

    public ImmutableArray<string> Tags { get; init; } = [];

    public bool IsPinned { get; init; }

    public PasswordStrength Strength { get; init; } = PasswordStrength.Unknown;

    public DateTimeOffset? ExpiresAt { get; init; }
}

