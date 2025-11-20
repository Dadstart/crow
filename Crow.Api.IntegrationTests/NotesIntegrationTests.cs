using System.Net;
using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Xunit;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class NotesIntegrationTests : IntegrationTestBase
{
    public NotesIntegrationTests(CrowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateNote_ShouldReturnCreatedNote()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreateNoteDto("Test Note", "Test Content", ["tag1", "tag2"]);

        var response = await client.PostAsJsonAsync("/api/notes", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Equal("Test Note", note.Title);
        Assert.Equal("Test Content", note.Content);
        Assert.Equal(2, note.Tags.Count);
    }

    [Fact]
    public async Task GetNoteById_ShouldReturnNote_WhenExists()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreateNoteDto("Test Note", "Content", null);
        var createResponse = await client.PostAsJsonAsync("/api/notes", dto);
        createResponse.EnsureSuccessStatusCode();
        var createdNote = await createResponse.Content.ReadFromJsonAsync<Note>();

        var response = await client.GetAsync($"/api/notes/{createdNote!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Equal(createdNote.Id, note.Id);
    }

    [Fact]
    public async Task GetAllNotes_ShouldReturnAllNotes()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto1 = new CreateNoteDto("Note 1", "Content 1", null);
        var dto2 = new CreateNoteDto("Note 2", "Content 2", null);

        var create1 = await client.PostAsJsonAsync("/api/notes", dto1);
        create1.EnsureSuccessStatusCode();
        var create2 = await client.PostAsJsonAsync("/api/notes", dto2);
        create2.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.True(notes.Count >= 2);
    }

    [Fact]
    public async Task UpdateNote_ShouldUpdateNote()
    {
        var client = await GetAuthenticatedClientAsync();
        var createDto = new CreateNoteDto("Original", "Original Content", null);
        var createResponse = await client.PostAsJsonAsync("/api/notes", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdNote = await createResponse.Content.ReadFromJsonAsync<Note>();

        var updateDto = new UpdateNoteDto("Updated", "Updated Content", ["newtag"]);
        var updateResponse = await client.PutAsJsonAsync($"/api/notes/{createdNote!.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedNote = await updateResponse.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(updatedNote);
        Assert.Equal("Updated", updatedNote.Title);
        Assert.Equal("Updated Content", updatedNote.Content);
    }

    [Fact]
    public async Task DeleteNote_ShouldRemoveNote()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreateNoteDto("To Delete", "Content", null);
        var createResponse = await client.PostAsJsonAsync("/api/notes", dto);
        createResponse.EnsureSuccessStatusCode();
        var createdNote = await createResponse.Content.ReadFromJsonAsync<Note>();

        var deleteResponse = await client.DeleteAsync($"/api/notes/{createdNote!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/notes/{createdNote.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

