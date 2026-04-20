using Crow.Services;
using Xunit;

namespace Crow.Tests;

public sealed class AppFileStorageTests : IDisposable
{
    readonly string _root;

    public AppFileStorageTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "crow-appfilestorage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best effort cleanup for temp tests
        }
    }

    [Fact]
    public async Task WriteTextAsync_creates_nested_file_under_root()
    {
        var storage = new AppFileStorage(_root);
        await storage.WriteTextAsync(Path.Combine("exports", "sample.json"), "{\"a\":1}");

        var full = Path.Combine(_root, "exports", "sample.json");
        Assert.True(File.Exists(full));
        var text = await File.ReadAllTextAsync(full);
        Assert.Equal("{\"a\":1}", text);
    }

    [Fact]
    public async Task WriteTextAsync_rejects_paths_that_escape_root()
    {
        var storage = new AppFileStorage(_root);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.WriteTextAsync(Path.Combine("..", "outside.txt"), "x"));
    }

    [Fact]
    public async Task WriteTextAsync_rejects_rooted_relative_path()
    {
        var storage = new AppFileStorage(_root);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.WriteTextAsync(Path.GetPathRoot(_root)! + "absolute-fail.txt", "x"));
    }
}
