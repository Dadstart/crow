namespace Dadstart.Labs.Crow.Models.Dtos;

public record UpdateNoteDto(string? Title, string? Content, List<string>? Tags);

