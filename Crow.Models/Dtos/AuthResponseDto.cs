namespace Dadstart.Labs.Crow.Models.Dtos;

public record AuthResponseDto(string Token, string RefreshToken, string Username, Guid UserId);

