using System.Text.Json;
using Crow.Models;
using Crow.Services;
using Xunit;

namespace Crow.Tests;

public sealed class DataExportServiceTests
{
    [Fact]
    public async Task ExportAllToJsonAsync_includes_tasks_notes_and_metadata()
    {
        using var scope = new TestDatabaseScope();
        await scope.Tasks.AddAsync(new TaskItem
        {
            Title = "Export me",
            Description = "Task payload",
            Priority = 1,
            Tags = ["export"],
        });
        await scope.Notes.AddAsync(new NoteItem
        {
            Title = "Also export me",
            Content = "Note payload",
            Tags = ["export"],
        });

        var sut = new DataExportService(scope.Tasks, scope.Notes, scope.Storage);
        var outputPath = await sut.ExportAllToJsonAsync();

        Assert.True(File.Exists(outputPath));

        var json = await File.ReadAllTextAsync(outputPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(DataExportService.CurrentSchemaVersion, root.GetProperty("schemaVersion").GetInt32());
        Assert.True(root.GetProperty("exportedAt").GetDateTimeOffset() > DateTimeOffset.UtcNow.AddMinutes(-5));

        var tasks = root.GetProperty("tasks");
        Assert.Single(tasks.EnumerateArray());
        Assert.Equal("Export me", tasks[0].GetProperty("title").GetString());

        var notes = root.GetProperty("notes");
        Assert.Single(notes.EnumerateArray());
        Assert.Equal("Also export me", notes[0].GetProperty("title").GetString());
    }
}
