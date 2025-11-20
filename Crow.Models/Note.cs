namespace Dadstart.Labs.Crow.Models;

public record Note(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<string> Tags)
{
    public static Note Create(string title, string content, List<string>? tags = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new Note(
            Id: Guid.NewGuid(),
            Title: title,
            Content: content,
            CreatedAt: now,
            UpdatedAt: now,
            Tags: tags ?? []);
    }

    public Note WithUpdate(string? title = null, string? content = null, List<string>? tags = null)
    {
        return this with
        {
            Title = title ?? Title,
            Content = content ?? Content,
            Tags = tags ?? Tags,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}

