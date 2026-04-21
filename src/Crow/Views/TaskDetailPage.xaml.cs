using Crow.ViewModels;

namespace Crow.Views;

[QueryProperty(nameof(TaskIdQuery), "TaskId")]
public partial class TaskDetailPage : ContentPage
{
    string? _pendingTaskId;

    public string TaskIdQuery
    {
        set
        {
            _pendingTaskId = value;
            TryApplyQuery();
        }
    }

    public TaskDetailPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<TaskDetailViewModel>(this, TryApplyQuery);
    }

    void TryApplyQuery()
    {
        if (BindingContext is not TaskDetailViewModel vm)
            return;
        if (_pendingTaskId == null)
            return;

        if (!Guid.TryParse(_pendingTaskId, out var id) || id == Guid.Empty)
            vm.BeginNewTask();
        else
            _ = vm.LoadTaskAsync(id);

        _pendingTaskId = null;
    }

    async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not TaskDetailViewModel vm)
            return;

        await vm.SaveAsync().ConfigureAwait(false);
        await Shell.Current.GoToAsync("..").ConfigureAwait(false);
    }

    async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not TaskDetailViewModel vm || vm.IsNewTask || vm.CurrentTask == null)
            return;

        await vm.DeleteTaskAsync(vm.CurrentTask).ConfigureAwait(false);
        await Shell.Current.GoToAsync("..").ConfigureAwait(false);
    }
}
