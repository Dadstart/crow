using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string email, string password);
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    string? Token { get; }
    string? Username { get; }
    Guid? UserId { get; }
}

