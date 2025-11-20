using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Dadstart.Labs.Crow.Models.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dadstart.Labs.Crow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PasswordsController(IStorageService storageService, IEncryptionService encryptionService, PasswordFactory passwordFactory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PasswordResponseDto>>> GetAll()
    {
        var passwords = await storageService.GetAllPasswordsAsync();
        var response = passwords.Select(p => new PasswordResponseDto(
            p.Id,
            p.Title,
            p.Username,
            encryptionService.Decrypt(p.EncryptedPassword),
            p.Url,
            p.Notes,
            p.CreatedAt,
            p.UpdatedAt)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PasswordResponseDto>> GetById(Guid id)
    {
        var password = await storageService.GetPasswordByIdAsync(id);
        if (password == null)
            return NotFound();

        var response = new PasswordResponseDto(
            password.Id,
            password.Title,
            password.Username,
            encryptionService.Decrypt(password.EncryptedPassword),
            password.Url,
            password.Notes,
            password.CreatedAt,
            password.UpdatedAt);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<PasswordResponseDto>> Create([FromBody] CreatePasswordDto dto)
    {
        var encryptedPassword = encryptionService.Encrypt(dto.Password);
        var password = passwordFactory.Create(dto.Title, dto.Username, encryptedPassword, dto.Url, dto.Notes);
        var created = await storageService.CreatePasswordAsync(password);
        
        var response = new PasswordResponseDto(
            created.Id,
            created.Title,
            created.Username,
            dto.Password,
            created.Url,
            created.Notes,
            created.CreatedAt,
            created.UpdatedAt);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PasswordResponseDto>> Update(Guid id, [FromBody] UpdatePasswordDto dto)
    {
        var existing = await storageService.GetPasswordByIdAsync(id);
        if (existing == null)
            return NotFound();

        var encryptedPassword = dto.Password != null 
            ? encryptionService.Encrypt(dto.Password) 
            : existing.EncryptedPassword;
        
        var updated = passwordFactory.WithUpdate(existing, dto.Title, dto.Username, encryptedPassword, dto.Url, dto.Notes);
        var result = await storageService.UpdatePasswordAsync(id, updated);
        
        if (result == null)
            return NotFound();

        var response = new PasswordResponseDto(
            result.Id,
            result.Title,
            result.Username,
            dto.Password ?? encryptionService.Decrypt(result.EncryptedPassword),
            result.Url,
            result.Notes,
            result.CreatedAt,
            result.UpdatedAt);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await storageService.DeletePasswordAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

