namespace Crow.Models;

public sealed class CrowExportDocument
{
    public CrowExportDocument(
        int schemaVersion,
        DateTimeOffset exportedAt,
        IReadOnlyList<TaskItem> tasks,
        IReadOnlyList<NoteItem> notes)
    {
        SchemaVersion = schemaVersion;
        ExportedAt = exportedAt;
        Tasks = tasks;
        Notes = notes;
    }

    public int SchemaVersion { get; }

    public DateTimeOffset ExportedAt { get; }

    public IReadOnlyList<TaskItem> Tasks { get; }

    public IReadOnlyList<NoteItem> Notes { get; }
}
