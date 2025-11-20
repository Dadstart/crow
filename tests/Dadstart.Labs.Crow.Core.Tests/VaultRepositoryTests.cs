namespace Dadstart.Labs.Crow.Tests;

using System.Linq;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Server.Repositories;

public class VaultRepositoryTests
{
    readonly InMemoryVaultRepository _repository = new();

    [Fact]
    public async Task UpsertNote_PersistsData()
    {
        var mutation = new NoteMutation
        {
            Title = "note",
            RichTextBody = "body",
            Tags = ["security", "test"]
        };

        var note = await _repository.UpsertNoteAsync(mutation, CancellationToken.None);
        Assert.Equal("note", note.Title);

        var notes = await _repository.GetNotesAsync("sec", CancellationToken.None);
        Assert.Single(notes);
    }

    [Fact]
    public async Task UpsertReminder_SchedulesChronologically()
    {
        await _repository.UpsertReminderAsync(new ReminderMutation
        {
            Title = "later",
            Body = "later",
            ScheduledAt = DateTimeOffset.UtcNow.AddHours(1)
        }, CancellationToken.None);

        await _repository.UpsertReminderAsync(new ReminderMutation
        {
            Title = "sooner",
            Body = "soon",
            ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(10)
        }, CancellationToken.None);

        var reminders = await _repository.GetRemindersAsync(CancellationToken.None);
        Assert.Equal(["sooner", "later"], reminders.Select(r => r.Title));
    }
}

