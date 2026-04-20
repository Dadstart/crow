using System.Globalization;
using System.Text.Json;
using Crow.Models;
using Crow.Services;
using Microsoft.Data.Sqlite;

namespace Crow.Repositories;

public sealed class TaskRepository
{
    static readonly JsonSerializerOptions JsonOptions = new();

    readonly DatabaseService _database;

    public TaskRepository(DatabaseService database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Description, IsCompleted, DueDate, Priority, Tags, CreatedAt, UpdatedAt
            FROM TaskItems
            ORDER BY UpdatedAt DESC;
            """;

        var list = new List<TaskItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(ReadTask(reader));

        return list;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Description, IsCompleted, DueDate, Priority, Tags, CreatedAt, UpdatedAt
            FROM TaskItems
            WHERE Id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", id.ToString());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return ReadTask(reader);
    }

    public async Task AddAsync(TaskItem item, CancellationToken cancellationToken = default)
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
            INSERT INTO TaskItems (Id, Title, Description, IsCompleted, DueDate, Priority, Tags, CreatedAt, UpdatedAt)
            VALUES ($id, $title, $description, $isCompleted, $dueDate, $priority, $tags, $createdAt, $updatedAt);
            """;
        AddTaskParameters(cmd, item);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(TaskItem item, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE TaskItems
            SET Title = $title,
                Description = $description,
                IsCompleted = $isCompleted,
                DueDate = $dueDate,
                Priority = $priority,
                Tags = $tags,
                CreatedAt = $createdAt,
                UpdatedAt = $updatedAt
            WHERE Id = $id;
            """;
        AddTaskParameters(cmd, item);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM TaskItems WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    static TaskItem ReadTask(SqliteDataReader reader)
    {
        var dueDateOrdinal = reader.GetOrdinal("DueDate");
        return new TaskItem
        {
            Id = Guid.Parse(reader.GetString(reader.GetOrdinal("Id"))),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            IsCompleted = reader.GetInt32(reader.GetOrdinal("IsCompleted")) != 0,
            DueDate = reader.IsDBNull(dueDateOrdinal)
                ? null
                : DateTime.Parse(reader.GetString(dueDateOrdinal), null, DateTimeStyles.RoundtripKind),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            Tags = DeserializeTags(reader.GetString(reader.GetOrdinal("Tags"))),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")), null, DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")), null, DateTimeStyles.RoundtripKind),
        };
    }

    static void AddTaskParameters(SqliteCommand cmd, TaskItem item)
    {
        cmd.Parameters.AddWithValue("$id", item.Id.ToString());
        cmd.Parameters.AddWithValue("$title", item.Title);
        cmd.Parameters.AddWithValue("$description", item.Description);
        cmd.Parameters.AddWithValue("$isCompleted", item.IsCompleted ? 1 : 0);
        cmd.Parameters.AddWithValue("$dueDate", item.DueDate.HasValue ? item.DueDate.Value.ToString("o", CultureInfo.InvariantCulture) : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$priority", item.Priority);
        cmd.Parameters.AddWithValue("$tags", SerializeTags(item.Tags));
        cmd.Parameters.AddWithValue("$createdAt", item.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$updatedAt", item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture));
    }

    static string SerializeTags(List<string> tags) =>
        JsonSerializer.Serialize(tags, JsonOptions);

    static List<string> DeserializeTags(string json) =>
        JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
}
