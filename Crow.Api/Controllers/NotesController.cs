using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dadstart.Labs.Crow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController(IStorageService storageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Note>>> GetAll()
    {
        var notes = await storageService.GetAllNotesAsync();
        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetById(Guid id)
    {
        var note = await storageService.GetNoteByIdAsync(id);
        if (note == null)
            return NotFound();

        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Create([FromBody] CreateNoteDto dto)
    {
        var note = Note.Create(dto.Title, dto.Content, dto.Tags);
        var created = await storageService.CreateNoteAsync(note);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Note>> Update(Guid id, [FromBody] UpdateNoteDto dto)
    {
        var existing = await storageService.GetNoteByIdAsync(id);
        if (existing == null)
            return NotFound();

        var updated = existing.WithUpdate(dto.Title, dto.Content, dto.Tags);
        var result = await storageService.UpdateNoteAsync(id, updated);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await storageService.DeleteNoteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

