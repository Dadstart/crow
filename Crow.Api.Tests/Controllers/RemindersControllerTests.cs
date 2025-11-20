using Dadstart.Labs.Crow.Api.Controllers;
using Dadstart.Labs.Crow.Api.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Dadstart.Labs.Crow.Models.Factories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Dadstart.Labs.Crow.Api.Tests.Controllers;

public class RemindersControllerTests
{
    private readonly Mock<IStorageService> _mockStorage;
    private readonly ReminderFactory _reminderFactory;
    private readonly RemindersController _controller;

    public RemindersControllerTests()
    {
        _mockStorage = new Mock<IStorageService>();
        _reminderFactory = new ReminderFactory(TimeProvider.System);
        _controller = new RemindersController(_mockStorage.Object, _reminderFactory);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllReminders()
    {
        var reminders = new List<Reminder>
        {
            _reminderFactory.Create("Rem 1", "Desc 1"),
            _reminderFactory.Create("Rem 2", "Desc 2")
        };
        _mockStorage.Setup(s => s.GetAllRemindersAsync()).ReturnsAsync(reminders);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedReminders = Assert.IsAssignableFrom<List<Reminder>>(okResult.Value);
        Assert.Equal(2, returnedReminders.Count);
    }

    [Fact]
    public async Task GetById_ShouldReturnReminder_WhenExists()
    {
        var reminder = _reminderFactory.Create("Test", "Desc");
        _mockStorage.Setup(s => s.GetReminderByIdAsync(reminder.Id)).ReturnsAsync(reminder);

        var result = await _controller.GetById(reminder.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedReminder = Assert.IsType<Reminder>(okResult.Value);
        Assert.Equal(reminder.Id, returnedReminder.Id);
    }

    [Fact]
    public async Task Create_ShouldCreateReminder()
    {
        var dueDate = TimeProvider.System.GetUtcNow().AddDays(1);
        var dto = new CreateReminderDto("Test", "Description", dueDate);
        var reminder = _reminderFactory.Create(dto.Title, dto.Description, dto.DueDate);
        _mockStorage.Setup(s => s.CreateReminderAsync(It.IsAny<Reminder>())).ReturnsAsync(reminder);

        var result = await _controller.Create(dto);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedReminder = Assert.IsType<Reminder>(createdAtResult.Value);
        Assert.Equal("Test", returnedReminder.Title);
    }

    [Fact]
    public async Task Update_ShouldUpdateReminder_WhenExists()
    {
        var reminder = _reminderFactory.Create("Original", "Original Desc");
        var dto = new UpdateReminderDto("Updated", "Updated Desc", null, true);
        var updated = _reminderFactory.WithUpdate(reminder, dto.Title, dto.Description, dto.DueDate, dto.IsCompleted);
        
        _mockStorage.Setup(s => s.GetReminderByIdAsync(reminder.Id)).ReturnsAsync(reminder);
        _mockStorage.Setup(s => s.UpdateReminderAsync(reminder.Id, It.IsAny<Reminder>())).ReturnsAsync(updated);

        var result = await _controller.Update(reminder.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedReminder = Assert.IsType<Reminder>(okResult.Value);
        Assert.Equal("Updated", returnedReminder.Title);
        Assert.True(returnedReminder.IsCompleted);
    }

    [Fact]
    public async Task Delete_ShouldDeleteReminder_WhenExists()
    {
        var id = Guid.NewGuid();
        _mockStorage.Setup(s => s.DeleteReminderAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }
}

