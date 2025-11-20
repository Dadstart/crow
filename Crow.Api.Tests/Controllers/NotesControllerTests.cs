using Dadstart.Labs.Crow.Api.Controllers;
using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Controllers;

public class NotesControllerTests
{
    private readonly Mock<IStorageService> _mockStorage;
    private readonly NotesController _controller;

    public NotesControllerTests()
    {
        _mockStorage = new Mock<IStorageService>();
        _controller = new NotesController(_mockStorage.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllNotes()
    {
        var notes = new List<Note>
        {
            Note.Create("Note 1", "Content 1"),
            Note.Create("Note 2", "Content 2")
        };
        _mockStorage.Setup(s => s.GetAllNotesAsync()).ReturnsAsync(notes);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedNotes = Assert.IsAssignableFrom<List<Note>>(okResult.Value);
        Assert.Equal(2, returnedNotes.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnNote_WhenExists()
    {
        var note = Note.Create("Test", "Content");
        _mockStorage.Setup(s => s.GetNoteByIdAsync(note.Id)).ReturnsAsync(note);

        var result = await _controller.GetById(note.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal(note.Id, returnedNote.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        _mockStorage.Setup(s => s.GetNoteByIdAsync(id)).ReturnsAsync((Note?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldCreateNote()
    {
        var dto = new CreateNoteDto("Test", "Content", ["tag1"]);
        var note = Note.Create(dto.Title, dto.Content, dto.Tags);
        _mockStorage.Setup(s => s.CreateNoteAsync(It.IsAny<Note>())).ReturnsAsync(note);

        var result = await _controller.Create(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedNote = Assert.IsType<Note>(createdAtResult.Value);
        Assert.Equal("Test", returnedNote.Title);
        _mockStorage.Verify(s => s.CreateNoteAsync(It.IsAny<Note>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldUpdateNote_WhenExists()
    {
        var note = Note.Create("Original", "Original Content");
        var dto = new UpdateNoteDto("Updated", "Updated Content", null);
        var updated = note.WithUpdate(dto.Title, dto.Content, dto.Tags);
        
        _mockStorage.Setup(s => s.GetNoteByIdAsync(note.Id)).ReturnsAsync(note);
        _mockStorage.Setup(s => s.UpdateNoteAsync(note.Id, It.IsAny<Note>())).ReturnsAsync(updated);

        var result = await _controller.Update(note.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal("Updated", returnedNote.Title);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateNoteDto("Updated", "Content", null);
        _mockStorage.Setup(s => s.GetNoteByIdAsync(id)).ReturnsAsync((Note?)null);

        var result = await _controller.Update(id, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ShouldDeleteNote_WhenExists()
    {
        var id = Guid.NewGuid();
        _mockStorage.Setup(s => s.DeleteNoteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockStorage.Verify(s => s.DeleteNoteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        _mockStorage.Setup(s => s.DeleteNoteAsync(id)).ReturnsAsync(false);

        var result = await _controller.Delete(id);

        Assert.IsType<NotFoundResult>(result);
    }
}

