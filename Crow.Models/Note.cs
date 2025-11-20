namespace Dadstart.Labs.Crow.Models;

public record Note(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<string> Tags);

