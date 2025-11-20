using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.Api.Services;

public class AuthService : IAuthService
{
    private readonly IStorageService _storageService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IStorageService storageService, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _storageService = storageService;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _storageService.GetUserByUsernameAsync(dto.Username) != null)
            return null;

        if (await _storageService.GetUserByEmailAsync(dto.Email) != null)
            return null;

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var user = User.Create(dto.Username, dto.Email, passwordHash);
        var created = await _storageService.CreateUserAsync(user);

        var token = _jwtTokenService.GenerateToken(created.Id, created.Username);
        return new AuthResponseDto(token, created.Username, created.Id);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _storageService.GetUserByUsernameAsync(dto.Username);
        if (user == null)
            return null;

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        var token = _jwtTokenService.GenerateToken(user.Id, user.Username);
        return new AuthResponseDto(token, user.Username, user.Id);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _storageService.GetUserByIdAsync(userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _storageService.GetUserByUsernameAsync(username);
    }
}

