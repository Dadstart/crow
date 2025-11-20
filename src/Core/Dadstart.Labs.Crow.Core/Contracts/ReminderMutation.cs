namespace Dadstart.Labs.Crow.Contracts;

using Dadstart.Labs.Crow.Models;
using System.Collections.Immutable;

public sealed record class ReminderMutation
{
    public Guid? Id { get; init; }

    public required string Title { get; init; }

    public string Body { get; init; } = string.Empty;

    public ImmutableArray<string> Tags { get; init; } = [];

    public bool IsPinned { get; init; }

    public DateTimeOffset ScheduledAt { get; init; } = DateTimeOffset.UtcNow.AddMinutes(5);

    public ReminderRecurrence Recurrence { get; init; }

    public TimeSpan? SnoozeDuration { get; init; }
}

