namespace Dadstart.Labs.Crow.Models.Factories;

public class NoteFactory
{
    private readonly TimeProvider _timeProvider;

    public NoteFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Note Create(string title, string content, List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        var now = _timeProvider.GetUtcNow();
        return new Note(
            Id: Guid.NewGuid(),
            Title: title.Trim(),
            Content: content.Trim(),
            CreatedAt: now,
            UpdatedAt: now,
            Tags: tags ?? []);
    }

    public Note WithUpdate(Note note, string? title = null, string? content = null, List<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (title != null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (content != null && string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        return note with
        {
            Title = title?.Trim() ?? note.Title,
            Content = content?.Trim() ?? note.Content,
            Tags = tags ?? note.Tags,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }
}

