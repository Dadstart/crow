using System.Collections.ObjectModel;
using System.Windows.Input;
using Crow.Models;
using Crow.Repositories;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class TaskListViewModel : BaseViewModel
{
    readonly TaskRepository _taskRepository;

    ObservableCollection<TaskItem> _tasks = [];

    public TaskListViewModel(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;

        LoadTasksCommand = new Command(async () => await LoadTasksAsync().ConfigureAwait(false));
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

    public ObservableCollection<TaskItem> Tasks
    {
        get => _tasks;
        private set
        {
            if (ReferenceEquals(_tasks, value))
                return;
            _tasks = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadTasksCommand { get; }

    public ICommand AddTaskCommand { get; }

    public ICommand UpdateTaskCommand { get; }

    public ICommand DeleteTaskCommand { get; }

    public ICommand ToggleCompletionCommand { get; }

    public async Task LoadTasksAsync()
    {
        var items = await _taskRepository.GetAllAsync().ConfigureAwait(false);
        Tasks = new ObservableCollection<TaskItem>(items);
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        await _taskRepository.AddAsync(task).ConfigureAwait(false);
        await LoadTasksAsync().ConfigureAwait(false);
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task).ConfigureAwait(false);
        await LoadTasksAsync().ConfigureAwait(false);
    }

    public async Task DeleteTaskAsync(TaskItem task)
    {
        await _taskRepository.DeleteAsync(task.Id).ConfigureAwait(false);
        await LoadTasksAsync().ConfigureAwait(false);
    }

    public async Task ToggleCompletionAsync(TaskItem task)
    {
        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task).ConfigureAwait(false);
        await LoadTasksAsync().ConfigureAwait(false);
    }
}
