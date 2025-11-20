namespace Dadstart.Labs.Crow.Models.Dtos;

public record CreateNoteDto(string Title, string Content, List<string>? Tags);

