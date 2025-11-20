using System.Net;
using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;
using Xunit;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class RemindersIntegrationTests : IntegrationTestBase
{
    public RemindersIntegrationTests(CrowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateReminder_ShouldReturnCreatedReminder()
    {
        var client = await GetAuthenticatedClientAsync();
        var dueDate = DateTime.UtcNow.AddDays(1);
        var dto = new CreateReminderDto("Test Reminder", "Description", dueDate);

        var response = await client.PostAsJsonAsync("/api/reminders", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var reminder = await response.Content.ReadFromJsonAsync<Reminder>();
        Assert.NotNull(reminder);
        Assert.Equal("Test Reminder", reminder.Title);
        Assert.Equal("Description", reminder.Description);
        Assert.Equal(dueDate, reminder.DueDate);
        Assert.False(reminder.IsCompleted);
    }

    [Fact]
    public async Task GetAllReminders_ShouldReturnAllReminders()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto1 = new CreateReminderDto("Reminder 1", "Desc 1", null);
        var dto2 = new CreateReminderDto("Reminder 2", "Desc 2", DateTime.UtcNow.AddDays(2));

        var create1 = await client.PostAsJsonAsync("/api/reminders", dto1);
        create1.EnsureSuccessStatusCode();
        var create2 = await client.PostAsJsonAsync("/api/reminders", dto2);
        create2.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/reminders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reminders = await response.Content.ReadFromJsonAsync<List<Reminder>>();
        Assert.NotNull(reminders);
        Assert.True(reminders.Count >= 2);
    }

    [Fact]
    public async Task UpdateReminder_ShouldUpdateReminder()
    {
        var client = await GetAuthenticatedClientAsync();
        var createDto = new CreateReminderDto("Original", "Original Desc", null);
        var createResponse = await client.PostAsJsonAsync("/api/reminders", createDto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Reminder>();

        var newDueDate = DateTime.UtcNow.AddDays(3);
        var updateDto = new UpdateReminderDto("Updated", "Updated Desc", newDueDate, true);
        var updateResponse = await client.PutAsJsonAsync($"/api/reminders/{created!.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<Reminder>();
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Updated Desc", updated.Description);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task DeleteReminder_ShouldRemoveReminder()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreateReminderDto("To Delete", "Desc", null);
        var createResponse = await client.PostAsJsonAsync("/api/reminders", dto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<Reminder>();

        var deleteResponse = await client.DeleteAsync($"/api/reminders/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/reminders/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

