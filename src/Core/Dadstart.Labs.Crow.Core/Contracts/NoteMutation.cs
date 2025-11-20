namespace Dadstart.Labs.Crow.Contracts;

using System.Collections.Immutable;

public sealed record class NoteMutation
{
    public Guid? Id { get; init; }

    public required string Title { get; init; }

    public required string RichTextBody { get; init; }

    public ImmutableArray<string> Tags { get; init; } = [];

    public bool IsPinned { get; init; }
}

