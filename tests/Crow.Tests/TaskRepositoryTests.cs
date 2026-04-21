using Crow.Models;
using Xunit;

namespace Crow.Tests;

public sealed class TaskRepositoryTests
{
    [Fact]
    public async Task AddAsync_and_GetByIdAsync_round_trip_task()
    {
        using var scope = new TestDatabaseScope();
        var task = new TaskItem
        {
            Title = "Write tests",
            Description = "Cover repository behavior",
            IsCompleted = false,
            Priority = 2,
            Tags = ["test", "repo"],
            DueDate = DateTime.UtcNow.AddDays(1),
        };

        await scope.Tasks.AddAsync(task);
        var loaded = await scope.Tasks.GetByIdAsync(task.Id);

        Assert.NotNull(loaded);
        Assert.Equal(task.Title, loaded.Title);
        Assert.Equal(task.Description, loaded.Description);
        Assert.Equal(task.Priority, loaded.Priority);
        Assert.Equal(task.Tags, loaded.Tags);
        Assert.NotNull(loaded.DueDate);
        Assert.Equal(task.DueDate?.Date, loaded.DueDate?.Date);
    }

    [Fact]
    public async Task UpdateAsync_persists_changed_values()
    {
        using var scope = new TestDatabaseScope();
        var task = new TaskItem
        {
            Title = "Old title",
            Description = "Old desc",
            Priority = 0,
            Tags = ["initial"],
        };

        await scope.Tasks.AddAsync(task);

        task.Title = "New title";
        task.Description = "New desc";
        task.IsCompleted = true;
        task.Priority = 1;
        task.Tags = ["updated"];
        task.UpdatedAt = DateTime.UtcNow;
        await scope.Tasks.UpdateAsync(task);

        var loaded = await scope.Tasks.GetByIdAsync(task.Id);
        Assert.NotNull(loaded);
        Assert.Equal("New title", loaded.Title);
        Assert.Equal("New desc", loaded.Description);
        Assert.True(loaded.IsCompleted);
        Assert.Equal(1, loaded.Priority);
        Assert.Equal(["updated"], loaded.Tags);
    }

    [Fact]
    public async Task UpdateAsync_can_clear_existing_due_date()
    {
        using var scope = new TestDatabaseScope();
        var task = new TaskItem
        {
            Title = "Task with due date",
            Description = "Will clear due date",
            Priority = 1,
            Tags = ["due"],
            DueDate = DateTime.UtcNow.Date.AddDays(2),
        };

        await scope.Tasks.AddAsync(task);

        task.DueDate = null;
        task.UpdatedAt = DateTime.UtcNow;
        await scope.Tasks.UpdateAsync(task);

        var loaded = await scope.Tasks.GetByIdAsync(task.Id);
        Assert.NotNull(loaded);
        Assert.Null(loaded.DueDate);
    }

    [Fact]
    public async Task SearchByTitleOrDescriptionAsync_matches_case_insensitively()
    {
        using var scope = new TestDatabaseScope();

        await scope.Tasks.AddAsync(new TaskItem
        {
            Title = "Buy Apples",
            Description = "From market",
            Priority = 0,
            Tags = [],
        });
        await scope.Tasks.AddAsync(new TaskItem
        {
            Title = "Clean desk",
            Description = "Sort APPLES documents",
            Priority = 0,
            Tags = [],
        });

        var results = await scope.Tasks.SearchByTitleOrDescriptionAsync(" apples ");

        Assert.Equal(2, results.Count);
    }
}
