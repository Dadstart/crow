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
        var now = _timeProvider.GetUtcNow();
        return new Note(
            Id: Guid.NewGuid(),
            Title: title,
            Content: content,
            CreatedAt: now,
            UpdatedAt: now,
            Tags: tags ?? []);
    }

    public Note WithUpdate(Note note, string? title = null, string? content = null, List<string>? tags = null)
    {
        return note with
        {
            Title = title ?? note.Title,
            Content = content ?? note.Content,
            Tags = tags ?? note.Tags,
            UpdatedAt = _timeProvider.GetUtcNow()
        };
    }
}

