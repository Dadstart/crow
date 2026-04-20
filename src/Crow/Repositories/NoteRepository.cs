using System.Globalization;
using System.Text.Json;
using Crow.Models;
using Crow.Services;
using Microsoft.Data.Sqlite;

namespace Crow.Repositories;

public sealed class NoteRepository
{
    static readonly JsonSerializerOptions JsonOptions = new();

    readonly DatabaseService _database;

    public NoteRepository(DatabaseService database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<NoteItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Content, Tags, CreatedAt, UpdatedAt
            FROM NoteItems
            ORDER BY UpdatedAt DESC;
            """;

        var list = new List<NoteItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(ReadNote(reader));

        return list;
    }

    public async Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Content, Tags, CreatedAt, UpdatedAt
            FROM NoteItems
            WHERE Id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return ReadNote(reader);
    }

    public async Task AddAsync(NoteItem item, CancellationToken cancellationToken = default)
    {
        if (item.Id == Guid.Empty)
            item.Id = Guid.NewGuid();

        var now = DateTime.UtcNow;
        if (item.CreatedAt == default)
            item.CreatedAt = now;
        if (item.UpdatedAt == default)
            item.UpdatedAt = now;

        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO NoteItems (Id, Title, Content, Tags, CreatedAt, UpdatedAt)
            VALUES ($id, $title, $content, $tags, $createdAt, $updatedAt);
            """;
        AddNoteParameters(cmd, item);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(NoteItem item, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE NoteItems
            SET Title = $title,
                Content = $content,
                Tags = $tags,
                CreatedAt = $createdAt,
                UpdatedAt = $updatedAt
            WHERE Id = $id;
            """;
        AddNoteParameters(cmd, item);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM NoteItems WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    static NoteItem ReadNote(SqliteDataReader reader)
    {
        return new NoteItem
        {
            Id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id"))),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Content = reader.GetString(reader.GetOrdinal("Content")),
            Tags = DeserializeTags(reader.GetString(reader.GetOrdinal("Tags"))),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")), null, DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")), null, DateTimeStyles.RoundtripKind),
        };
    }

    static void AddNoteParameters(SqliteCommand cmd, NoteItem item)
    {
        cmd.Parameters.AddWithValue("$id", item.Id.ToString());
        cmd.Parameters.AddWithValue("$title", item.Title);
        cmd.Parameters.AddWithValue("$content", item.Content);
        cmd.Parameters.AddWithValue("$tags", SerializeTags(item.Tags));
        cmd.Parameters.AddWithValue("$createdAt", item.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$updatedAt", item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture));
    }

    static string SerializeTags(List<string> tags) =>
        JsonSerializer.Serialize(tags, JsonOptions);

    static List<string> DeserializeTags(string json) =>
        JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
}
