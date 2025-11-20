using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.Api.Tests.Services;

public class InMemoryStorageServiceTests
{
    private readonly IStorageService _storageService;

    public InMemoryStorageServiceTests()
    {
        _storageService = new InMemoryStorageService();
    }

    [Fact]
    public async Task CreateNote_ShouldAddNoteToStorage()
    {
        var note = Note.Create("Test Note", "Test Content", ["tag1", "tag2"]);

        var result = await _storageService.CreateNoteAsync(note);

        Assert.Equal(note, result);
        var retrieved = await _storageService.GetNoteByIdAsync(note.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Note", retrieved.Title);
    }

    [Fact]
    public async Task GetNoteById_ShouldReturnNote_WhenExists()
    {
        var note = Note.Create("Test", "Content");
        await _storageService.CreateNoteAsync(note);

        var result = await _storageService.GetNoteByIdAsync(note.Id);

        Assert.NotNull(result);
        Assert.Equal(note.Id, result.Id);
    }

    [Fact]
    public async Task GetNoteById_ShouldReturnNull_WhenNotExists()
    {
        var result = await _storageService.GetNoteByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllNotes_ShouldReturnAllNotes()
    {
        var note1 = Note.Create("Note 1", "Content 1");
        var note2 = Note.Create("Note 2", "Content 2");
        await _storageService.CreateNoteAsync(note1);
        await _storageService.CreateNoteAsync(note2);

        var result = await _storageService.GetAllNotesAsync();

        Assert.Contains(note1, result);
        Assert.Contains(note2, result);
    }

    [Fact]
    public async Task UpdateNote_ShouldUpdateExistingNote()
    {
        var note = Note.Create("Original", "Original Content");
        await _storageService.CreateNoteAsync(note);
        var updated = note.WithUpdate("Updated", "Updated Content");

        var result = await _storageService.UpdateNoteAsync(note.Id, updated);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Title);
        Assert.Equal("Updated Content", result.Content);
    }

    [Fact]
    public async Task UpdateNote_ShouldReturnNull_WhenNotExists()
    {
        var note = Note.Create("Test", "Content");
        var result = await _storageService.UpdateNoteAsync(Guid.NewGuid(), note);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteNote_ShouldRemoveNote()
    {
        var note = Note.Create("Test", "Content");
        await _storageService.CreateNoteAsync(note);

        var deleted = await _storageService.DeleteNoteAsync(note.Id);
        var retrieved = await _storageService.GetNoteByIdAsync(note.Id);

        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteNote_ShouldReturnFalse_WhenNotExists()
    {
        var deleted = await _storageService.DeleteNoteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public async Task CreatePassword_ShouldAddPasswordToStorage()
    {
        var password = Password.Create("Test", "user", "encrypted123", "https://example.com", "Notes");

        var result = await _storageService.CreatePasswordAsync(password);

        Assert.Equal(password, result);
        var retrieved = await _storageService.GetPasswordByIdAsync(password.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved.Title);
    }

    [Fact]
    public async Task GetAllPasswords_ShouldReturnAllPasswords()
    {
        var pwd1 = Password.Create("Pwd 1", "user1", "enc1");
        var pwd2 = Password.Create("Pwd 2", "user2", "enc2");
        await _storageService.CreatePasswordAsync(pwd1);
        await _storageService.CreatePasswordAsync(pwd2);

        var result = await _storageService.GetAllPasswordsAsync();

        Assert.Contains(pwd1, result);
        Assert.Contains(pwd2, result);
    }

    [Fact]
    public async Task UpdatePassword_ShouldUpdateExistingPassword()
    {
        var password = Password.Create("Original", "user", "enc");
        await _storageService.CreatePasswordAsync(password);
        var updated = password.WithUpdate("Updated", "newuser", "newenc");

        var result = await _storageService.UpdatePasswordAsync(password.Id, updated);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Title);
        Assert.Equal("newuser", result.Username);
    }

    [Fact]
    public async Task DeletePassword_ShouldRemovePassword()
    {
        var password = Password.Create("Test", "user", "enc");
        await _storageService.CreatePasswordAsync(password);

        var deleted = await _storageService.DeletePasswordAsync(password.Id);
        var retrieved = await _storageService.GetPasswordByIdAsync(password.Id);

        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task CreateReminder_ShouldAddReminderToStorage()
    {
        var reminder = Reminder.Create("Test", "Description", DateTimeOffset.UtcNow.AddDays(1));

        var result = await _storageService.CreateReminderAsync(reminder);

        Assert.Equal(reminder, result);
        var retrieved = await _storageService.GetReminderByIdAsync(reminder.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test", retrieved.Title);
    }

    [Fact]
    public async Task GetAllReminders_ShouldReturnAllReminders()
    {
        var rem1 = Reminder.Create("Rem 1", "Desc 1");
        var rem2 = Reminder.Create("Rem 2", "Desc 2");
        await _storageService.CreateReminderAsync(rem1);
        await _storageService.CreateReminderAsync(rem2);

        var result = await _storageService.GetAllRemindersAsync();

        Assert.Contains(rem1, result);
        Assert.Contains(rem2, result);
    }

    [Fact]
    public async Task UpdateReminder_ShouldUpdateExistingReminder()
    {
        var reminder = Reminder.Create("Original", "Original Desc");
        await _storageService.CreateReminderAsync(reminder);
        var updated = reminder.WithUpdate("Updated", "Updated Desc", null, true);

        var result = await _storageService.UpdateReminderAsync(reminder.Id, updated);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Title);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task DeleteReminder_ShouldRemoveReminder()
    {
        var reminder = Reminder.Create("Test", "Desc");
        await _storageService.CreateReminderAsync(reminder);

        var deleted = await _storageService.DeleteReminderAsync(reminder.Id);
        var retrieved = await _storageService.GetReminderByIdAsync(reminder.Id);

        Assert.True(deleted);
        Assert.Null(retrieved);
    }
}

