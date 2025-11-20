using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Factories;

namespace Dadstart.Labs.Crow.Models.Tests;

public class ReminderTests
{
    private readonly ReminderFactory _factory = new(TimeProvider.System);

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var dueDate = TimeProvider.System.GetUtcNow().AddDays(1);
        var reminder = _factory.Create("Test", "Description", dueDate);

        Assert.NotEqual(Guid.Empty, reminder.Id);
        Assert.Equal("Test", reminder.Title);
        Assert.Equal("Description", reminder.Description);
        Assert.Equal(dueDate, reminder.DueDate);
        Assert.False(reminder.IsCompleted);
        Assert.True(reminder.CreatedAt <= TimeProvider.System.GetUtcNow());
    }

    [Fact]
    public void Create_ShouldAllowNullOptionalFields()
    {
        var reminder = _factory.Create("Test", null, null);

        Assert.Null(reminder.Description);
        Assert.Null(reminder.DueDate);
    }

    [Fact]
    public void WithUpdate_ShouldUpdateSpecifiedFields()
    {
        var original = _factory.Create("Original", "Original Desc", TimeProvider.System.GetUtcNow().AddDays(1));
        var newDueDate = TimeProvider.System.GetUtcNow().AddDays(2);
        var updated = _factory.WithUpdate(original, "Updated", "Updated Desc", newDueDate, true);

        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Updated Desc", updated.Description);
        Assert.Equal(newDueDate, updated.DueDate);
        Assert.True(updated.IsCompleted);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public void MarkCompleted_ShouldSetIsCompletedToTrue()
    {
        var reminder = _factory.Create("Test", "Desc");
        var completed = _factory.MarkCompleted(reminder);

        Assert.True(completed.IsCompleted);
        Assert.False(reminder.IsCompleted);
        Assert.True(completed.UpdatedAt > reminder.UpdatedAt);
    }

    [Fact]
    public void MarkIncomplete_ShouldSetIsCompletedToFalse()
    {
        var reminder = _factory.Create("Test", "Desc");
        var completed = _factory.MarkCompleted(reminder);
        var incomplete = _factory.MarkIncomplete(completed);

        Assert.False(incomplete.IsCompleted);
        Assert.True(completed.IsCompleted);
        Assert.True(incomplete.UpdatedAt > completed.UpdatedAt);
    }
}

