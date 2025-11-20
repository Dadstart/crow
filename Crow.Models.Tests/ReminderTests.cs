using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.Models.Tests;

public class ReminderTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var dueDate = DateTime.UtcNow.AddDays(1);
        var reminder = Reminder.Create("Test", "Description", dueDate);

        Assert.NotEqual(Guid.Empty, reminder.Id);
        Assert.Equal("Test", reminder.Title);
        Assert.Equal("Description", reminder.Description);
        Assert.Equal(dueDate, reminder.DueDate);
        Assert.False(reminder.IsCompleted);
        Assert.True(reminder.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldAllowNullOptionalFields()
    {
        var reminder = Reminder.Create("Test", null, null);

        Assert.Null(reminder.Description);
        Assert.Null(reminder.DueDate);
    }

    [Fact]
    public void WithUpdate_ShouldUpdateSpecifiedFields()
    {
        var original = Reminder.Create("Original", "Original Desc", DateTime.UtcNow.AddDays(1));
        var newDueDate = DateTime.UtcNow.AddDays(2);
        var updated = original.WithUpdate("Updated", "Updated Desc", newDueDate, true);

        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Updated Desc", updated.Description);
        Assert.Equal(newDueDate, updated.DueDate);
        Assert.True(updated.IsCompleted);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public void MarkCompleted_ShouldSetIsCompletedToTrue()
    {
        var reminder = Reminder.Create("Test", "Desc");
        var completed = reminder.MarkCompleted();

        Assert.True(completed.IsCompleted);
        Assert.False(reminder.IsCompleted);
        Assert.True(completed.UpdatedAt > reminder.UpdatedAt);
    }

    [Fact]
    public void MarkIncomplete_ShouldSetIsCompletedToFalse()
    {
        var reminder = Reminder.Create("Test", "Desc").MarkCompleted();
        var incomplete = reminder.MarkIncomplete();

        Assert.False(incomplete.IsCompleted);
        Assert.True(reminder.IsCompleted);
        Assert.True(incomplete.UpdatedAt > reminder.UpdatedAt);
    }
}

