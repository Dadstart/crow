namespace Crow.Models;

public sealed class SearchResultItem
{
    public SearchResultKind Kind { get; init; }

    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Preview { get; init; } = string.Empty;

    public DateTime UpdatedAt { get; init; }

    public string KindLabel => Kind == SearchResultKind.Task ? "Task" : "Note";
}
