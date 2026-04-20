using Crow.Models;
using Xunit;

namespace Crow.Tests;

public sealed class NoteRepositoryTests
{
    [Fact]
    public async Task AddAsync_and_GetAllAsync_returns_saved_note()
    {
        using var scope = new TestDatabaseScope();
        var note = new NoteItem
        {
            Title = "Design notes",
            Content = "Repository test coverage",
            Tags = ["dev", "tests"],
        };

        await scope.Notes.AddAsync(note);
        var all = await scope.Notes.GetAllAsync();

        Assert.Single(all);
        Assert.Equal(note.Title, all[0].Title);
        Assert.Equal(note.Content, all[0].Content);
        Assert.Equal(note.Tags, all[0].Tags);
    }

    [Fact]
    public async Task UpdateAsync_and_DeleteAsync_apply_changes()
    {
        using var scope = new TestDatabaseScope();
        var note = new NoteItem
        {
            Title = "Draft",
            Content = "Initial",
            Tags = ["todo"],
        };

        await scope.Notes.AddAsync(note);
        note.Content = "Final";
        note.Tags = ["done"];
        note.UpdatedAt = DateTime.UtcNow;
        await scope.Notes.UpdateAsync(note);

        var updated = await scope.Notes.GetByIdAsync(note.Id);
        Assert.NotNull(updated);
        Assert.Equal("Final", updated.Content);
        Assert.Equal(["done"], updated.Tags);

        await scope.Notes.DeleteAsync(note.Id);
        var deleted = await scope.Notes.GetByIdAsync(note.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task SearchByTitleOrContentAsync_matches_case_insensitively()
    {
        using var scope = new TestDatabaseScope();
        await scope.Notes.AddAsync(new NoteItem
        {
            Title = "Roadmap",
            Content = "Milestone Alpha",
            Tags = [],
        });
        await scope.Notes.AddAsync(new NoteItem
        {
            Title = "Meeting",
            Content = "Discuss alpha tasks",
            Tags = [],
        });

        var results = await scope.Notes.SearchByTitleOrContentAsync("ALPHA");

        Assert.Equal(2, results.Count);
    }
}
