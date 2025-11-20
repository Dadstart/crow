using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Factories;

namespace Dadstart.Labs.Crow.Models.Tests;

public class NoteTests
{
    private readonly NoteFactory _factory = new(TimeProvider.System);

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var tags = new List<string> { "tag1", "tag2" };
        var note = _factory.Create("Test Title", "Test Content", tags);

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal("Test Title", note.Title);
        Assert.Equal("Test Content", note.Content);
        Assert.Equal(tags, note.Tags);
        Assert.True(note.CreatedAt <= TimeProvider.System.GetUtcNow());
        Assert.Equal(note.CreatedAt, note.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldUseEmptyTagsList_WhenNull()
    {
        var note = _factory.Create("Test", "Content", null);

        Assert.Empty(note.Tags);
    }

    [Fact]
    public void WithUpdate_ShouldUpdateSpecifiedFields()
    {
        var original = _factory.Create("Original", "Original Content", ["tag1"]);
        var updated = _factory.WithUpdate(original, "Updated", "Updated Content", ["tag2"]);

        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Updated Content", updated.Content);
        Assert.Equal(["tag2"], updated.Tags);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public void WithUpdate_ShouldPreserveOriginalValues_WhenNull()
    {
        var original = _factory.Create("Original", "Original Content", ["tag1"]);
        var updated = _factory.WithUpdate(original, null, null, null);

        Assert.Equal("Original", updated.Title);
        Assert.Equal("Original Content", updated.Content);
        Assert.Equal(["tag1"], updated.Tags);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }
}

