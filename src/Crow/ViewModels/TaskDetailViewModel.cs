using System.Globalization;
using System.Windows.Input;
using Crow.Models;
using Crow.Repositories;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class TaskDetailViewModel : BaseViewModel
{
    public static readonly string[] PriorityOptions = ["Low", "Medium", "High"];

    readonly TaskRepository _taskRepository;

    TaskItem? _currentTask;
    string _dueDateText = "";
    string _tagsText = "";
    bool _isNewTask;

    public TaskDetailViewModel(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;

        LoadTaskCommand = new Command<Guid>(
            async id => await LoadTaskAsync(id).ConfigureAwait(false),
            id => id != Guid.Empty);
        AddTaskCommand = new Command<TaskItem>(
            async task => await AddTaskAsync(task).ConfigureAwait(false),
            task => task is not null);
        UpdateTaskCommand = new Command<TaskItem>(
            async task => await UpdateTaskAsync(task).ConfigureAwait(false),
            task => task is not null);
        DeleteTaskCommand = new Command<TaskItem>(
            async task => await DeleteTaskAsync(task).ConfigureAwait(false),
            task => task is not null);
        ToggleCompletionCommand = new Command<TaskItem>(
            async task => await ToggleCompletionAsync(task).ConfigureAwait(false),
            task => task is not null);
        SaveCommand = new Command(async () => await SaveAsync().ConfigureAwait(false), () => CurrentTask != null);
        DeleteCommand = new Command(async () => await DeleteCurrentAsync().ConfigureAwait(false), () => CurrentTask != null && !IsNewTask);
    }

    public TaskItem? CurrentTask
    {
        get => _currentTask;
        private set
        {
            if (ReferenceEquals(_currentTask, value))
                return;
            _currentTask = value;
            OnPropertyChanged();
            if (SaveCommand is Command saveCmd)
                saveCmd.ChangeCanExecute();
            if (DeleteCommand is Command delCmd)
                delCmd.ChangeCanExecute();
        }
    }

    public bool IsNewTask
    {
        get => _isNewTask;
        private set
        {
            if (_isNewTask == value)
                return;
            _isNewTask = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageTitle));
            if (DeleteCommand is Command deleteCmd)
                deleteCmd.ChangeCanExecute();
        }
    }

    public string PageTitle => IsNewTask ? "New task" : "Edit task";

    public string TagsText
    {
        get => _tagsText;
        set
        {
            if (_tagsText == value)
                return;
            _tagsText = value;
            OnPropertyChanged();
        }
    }

    public string DueDateText
    {
        get => _dueDateText;
        set
        {
            if (_dueDateText == value)
                return;
            _dueDateText = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadTaskCommand { get; }

    public ICommand AddTaskCommand { get; }

    public ICommand UpdateTaskCommand { get; }

    public ICommand DeleteTaskCommand { get; }

    public ICommand ToggleCompletionCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public void BeginNewTask()
    {
        IsNewTask = true;
        CurrentTask = new TaskItem
        {
            Title = "",
            Description = "",
            Tags = [],
            Priority = 0,
        };
        TagsText = "";
        DueDateText = "";
    }

    public async Task LoadTaskAsync(Guid id)
    {
        IsNewTask = false;
        CurrentTask = await _taskRepository.GetByIdAsync(id).ConfigureAwait(false);
        SyncEditorsFromTask();
    }

    public async Task SaveAsync()
    {
        if (CurrentTask == null)
            return;

        ApplyEditorsToTask();

        if (IsNewTask)
        {
            await AddTaskAsync(CurrentTask).ConfigureAwait(false);
            IsNewTask = false;
        }
        else
            await UpdateTaskAsync(CurrentTask).ConfigureAwait(false);

        SyncEditorsFromTask();
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        await _taskRepository.AddAsync(task).ConfigureAwait(false);
        CurrentTask = await _taskRepository.GetByIdAsync(task.Id).ConfigureAwait(false);
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task).ConfigureAwait(false);
        CurrentTask = await _taskRepository.GetByIdAsync(task.Id).ConfigureAwait(false);
    }

    public async Task DeleteTaskAsync(TaskItem task)
    {
        await _taskRepository.DeleteAsync(task.Id).ConfigureAwait(false);
        CurrentTask = null;
        TagsText = "";
        DueDateText = "";
    }

    public async Task ToggleCompletionAsync(TaskItem task)
    {
        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task).ConfigureAwait(false);
        CurrentTask = await _taskRepository.GetByIdAsync(task.Id).ConfigureAwait(false);
        SyncEditorsFromTask();
    }

    async Task DeleteCurrentAsync()
    {
        if (CurrentTask == null)
            return;
        await DeleteTaskAsync(CurrentTask).ConfigureAwait(false);
    }

    void ApplyEditorsToTask()
    {
        if (CurrentTask == null)
            return;

        CurrentTask.Tags = TagsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (string.IsNullOrWhiteSpace(DueDateText))
            CurrentTask.DueDate = null;
        else if (DateTime.TryParse(DueDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var due))
            CurrentTask.DueDate = DateTime.SpecifyKind(due.Date, DateTimeKind.Utc);
    }

    void SyncEditorsFromTask()
    {
        if (CurrentTask == null)
            return;

        TagsText = string.Join(", ", CurrentTask.Tags);
        DueDateText = CurrentTask.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
    }
}
