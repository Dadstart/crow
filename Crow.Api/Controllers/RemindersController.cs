using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dadstart.Labs.Crow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController(IStorageService storageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Reminder>>> GetAll()
    {
        var reminders = await storageService.GetAllRemindersAsync();
        return Ok(reminders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reminder>> GetById(Guid id)
    {
        var reminder = await storageService.GetReminderByIdAsync(id);
        if (reminder == null)
            return NotFound();

        return Ok(reminder);
    }

    [HttpPost]
    public async Task<ActionResult<Reminder>> Create([FromBody] CreateReminderDto dto)
    {
        var reminder = Reminder.Create(dto.Title, dto.Description, dto.DueDate);
        var created = await storageService.CreateReminderAsync(reminder);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Reminder>> Update(Guid id, [FromBody] UpdateReminderDto dto)
    {
        var existing = await storageService.GetReminderByIdAsync(id);
        if (existing == null)
            return NotFound();

        var updated = existing.WithUpdate(dto.Title, dto.Description, dto.DueDate, dto.IsCompleted);
        var result = await storageService.UpdateReminderAsync(id, updated);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await storageService.DeleteReminderAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

