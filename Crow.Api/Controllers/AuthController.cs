using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Dadstart.Labs.Crow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (result == null)
            return BadRequest("Username or email already exists");

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized("Invalid username or password");

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await authService.RefreshTokenAsync(dto.RefreshToken);
        if (result == null)
            return Unauthorized("Invalid or expired refresh token");

        return Ok(result);
    }
}

