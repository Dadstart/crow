using Crow.Models;
using Crow.Repositories;

namespace Crow.Services;

public sealed class SearchService
{
    const int PreviewMaxLength = 160;

    readonly TaskRepository _taskRepository;
    readonly NoteRepository _noteRepository;

    public SearchService(TaskRepository taskRepository, NoteRepository noteRepository)
    {
        _taskRepository = taskRepository;
        _noteRepository = noteRepository;
    }

    public async Task<IReadOnlyList<SearchResultItem>> SearchAllAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var tasks = await _taskRepository.SearchByTitleOrDescriptionAsync(query, cancellationToken).ConfigureAwait(false);
        var notes = await _noteRepository.SearchByTitleOrContentAsync(query, cancellationToken).ConfigureAwait(false);

        var combined = new List<SearchResultItem>(tasks.Count + notes.Count);

        foreach (var t in tasks)
        {
            combined.Add(new SearchResultItem
            {
                Kind = SearchResultKind.Task,
                Id = t.Id,
                Title = t.Title,
                Preview = TruncatePreview(t.Description),
                UpdatedAt = t.UpdatedAt,
            });
        }

        foreach (var n in notes)
        {
            combined.Add(new SearchResultItem
            {
                Kind = SearchResultKind.Note,
                Id = n.Id,
                Title = n.Title,
                Preview = TruncatePreview(n.Content),
                UpdatedAt = n.UpdatedAt,
            });
        }

        combined.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
        return combined;
    }

    static string TruncatePreview(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        if (text.Length <= PreviewMaxLength)
            return text;
        return string.Concat(text.AsSpan(0, PreviewMaxLength), "…");
    }
}
