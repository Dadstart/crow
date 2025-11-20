using System.Security.Claims;

namespace Dadstart.Labs.Crow.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string username);
    ClaimsPrincipal? ValidateToken(string token);
}

