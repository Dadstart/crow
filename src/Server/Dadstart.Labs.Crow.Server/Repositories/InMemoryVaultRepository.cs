namespace Dadstart.Labs.Crow.Server.Repositories;

using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class InMemoryVaultRepository : IVaultRepository
{
    readonly ConcurrentDictionary<Guid, SecureNote> _notes = new();
    readonly ConcurrentDictionary<Guid, PasswordEntry> _passwords = new();
    readonly ConcurrentDictionary<Guid, ReminderEntry> _reminders = new();

    public Task DeleteNoteAsync(Guid id, CancellationToken cancellationToken)
    {
        _notes.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task DeletePasswordAsync(Guid id, CancellationToken cancellationToken)
    {
        _passwords.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task DeleteReminderAsync(Guid id, CancellationToken cancellationToken)
    {
        _reminders.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<SecureNote?> GetNoteAsync(Guid id, CancellationToken cancellationToken)
    {
        _notes.TryGetValue(id, out var note);
        return Task.FromResult(note);
    }

    public Task<IReadOnlyList<SecureNote>> GetNotesAsync(string? query, CancellationToken cancellationToken)
    {
        var list = Filter(_notes, query);
        return Task.FromResult(list);
    }

    public Task<PasswordEntry?> GetPasswordAsync(Guid id, CancellationToken cancellationToken)
    {
        _passwords.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<PasswordEntry>> GetPasswordsAsync(string? query, CancellationToken cancellationToken)
    {
        var list = Filter(_passwords, query);
        return Task.FromResult(list);
    }

    public Task<ReminderEntry?> GetReminderAsync(Guid id, CancellationToken cancellationToken)
    {
        _reminders.TryGetValue(id, out var reminder);
        return Task.FromResult(reminder);
    }

    public Task<IReadOnlyList<ReminderEntry>> GetRemindersAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ReminderEntry> list = _reminders.Values
            .OrderBy(r => r.ScheduledAt)
            .ThenBy(r => r.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<SecureNote> UpsertNoteAsync(NoteMutation mutation, CancellationToken cancellationToken)
    {
        var note = _notes.AddOrUpdate(
            mutation.Id ?? Guid.NewGuid(),
            id => CreateNote(id, mutation),
            (id, existing) => CreateNote(id, mutation) with { CreatedAt = existing.CreatedAt });

        return Task.FromResult(note);
    }

    public Task<PasswordEntry> UpsertPasswordAsync(PasswordMutation mutation, CancellationToken cancellationToken)
    {
        var entry = _passwords.AddOrUpdate(
            mutation.Id ?? Guid.NewGuid(),
            id => CreatePassword(id, mutation),
            (id, existing) => CreatePassword(id, mutation) with { CreatedAt = existing.CreatedAt });

        return Task.FromResult(entry);
    }

    public Task<ReminderEntry> UpsertReminderAsync(ReminderMutation mutation, CancellationToken cancellationToken)
    {
        var reminder = _reminders.AddOrUpdate(
            mutation.Id ?? Guid.NewGuid(),
            id => CreateReminder(id, mutation),
            (id, existing) => CreateReminder(id, mutation) with { CreatedAt = existing.CreatedAt });

        return Task.FromResult(reminder);
    }

    static ImmutableArray<string> NormalizeTags(ImmutableArray<string> tags)
    {
        if (tags.IsDefaultOrEmpty)
        {
            return [];
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    static SecureNote CreateNote(Guid id, NoteMutation mutation)
        => new()
        {
            Id = id,
            Title = mutation.Title,
            RichTextBody = mutation.RichTextBody,
            Tags = NormalizeTags(mutation.Tags),
            IsPinned = mutation.IsPinned,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    static PasswordEntry CreatePassword(Guid id, PasswordMutation mutation)
        => new()
        {
            Id = id,
            Title = mutation.Title,
            Username = mutation.Username,
            Secret = mutation.Secret,
            ResourceUri = mutation.ResourceUri,
            Notes = mutation.Notes,
            Tags = NormalizeTags(mutation.Tags),
            Strength = mutation.Strength,
            ExpiresAt = mutation.ExpiresAt,
            IsPinned = mutation.IsPinned,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    static ReminderEntry CreateReminder(Guid id, ReminderMutation mutation)
        => new()
        {
            Id = id,
            Title = mutation.Title,
            Body = mutation.Body,
            Tags = NormalizeTags(mutation.Tags),
            IsPinned = mutation.IsPinned,
            ScheduledAt = mutation.ScheduledAt,
            Recurrence = mutation.Recurrence,
            SnoozeDuration = mutation.SnoozeDuration,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    static IReadOnlyList<TValue> Filter<TValue>(ConcurrentDictionary<Guid, TValue> source, string? query)
        where TValue : VaultItem
    {
        IEnumerable<TValue> results = source.Values;

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(item =>
                item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        return results
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ThenBy(n => n.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

