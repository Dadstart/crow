using Crow.Repositories;
using Crow.Services;

namespace Crow.Tests;

public sealed class TestDatabaseScope : IDisposable
{
    readonly string _root;

    public TestDatabaseScope()
    {
        _root = Path.Combine(Path.GetTempPath(), "crow-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);

        var dbPath = Path.Combine(_root, "crow-test.db");
        Database = new DatabaseService(dbPath);
        Database.Initialize();
        Tasks = new TaskRepository(Database);
        Notes = new NoteRepository(Database);
        Storage = new AppFileStorage(Path.Combine(_root, "appdata"));
    }

    public DatabaseService Database { get; }

    public TaskRepository Tasks { get; }

    public NoteRepository Notes { get; }

    public IAppFileStorage Storage { get; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // best effort temp cleanup
        }
    }
}
