using System.Text.Json;
using Crow.Models;
using Crow.Repositories;

namespace Crow.Services;

public sealed class DataExportService
{
    public const int CurrentSchemaVersion = 1;

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    readonly TaskRepository _taskRepository;
    readonly NoteRepository _noteRepository;
    readonly IAppFileStorage _fileStorage;

    public DataExportService(
        TaskRepository taskRepository,
        NoteRepository noteRepository,
        IAppFileStorage fileStorage)
    {
        _taskRepository = taskRepository;
        _noteRepository = noteRepository;
        _fileStorage = fileStorage;
    }

    /// Writes all tasks and notes to JSON under app storage and returns the absolute path of the file.
    public async Task<string> ExportAllToJsonAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var notes = await _noteRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var document = new CrowExportDocument(CurrentSchemaVersion, DateTimeOffset.UtcNow, tasks, notes);
        var json = JsonSerializer.Serialize(document, JsonOptions);

        var fileName = $"crow-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var relativePath = Path.Combine("exports", fileName);
        await _fileStorage.WriteTextAsync(relativePath, json, cancellationToken).ConfigureAwait(false);

        return Path.GetFullPath(Path.Combine(_fileStorage.RootDirectory, relativePath));
    }
}
