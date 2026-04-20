using System.Windows.Input;
using Crow.Models;
using Crow.Repositories;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class TaskDetailViewModel : BaseViewModel
{
    readonly TaskRepository _taskRepository;

    TaskItem? _currentTask;

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
        }
    }

    public ICommand LoadTaskCommand { get; }

    public ICommand AddTaskCommand { get; }

    public ICommand UpdateTaskCommand { get; }

    public ICommand DeleteTaskCommand { get; }

    public ICommand ToggleCompletionCommand { get; }

    public async Task LoadTaskAsync(Guid id)
    {
        CurrentTask = await _taskRepository.GetByIdAsync(id).ConfigureAwait(false);
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
    }

    public async Task ToggleCompletionAsync(TaskItem task)
    {
        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task).ConfigureAwait(false);
        CurrentTask = await _taskRepository.GetByIdAsync(task.Id).ConfigureAwait(false);
    }
}
