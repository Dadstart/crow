using Crow.Models;
using Crow.ViewModels;

namespace Crow.Views;

public partial class TaskListPage : ContentPage
{
    public TaskListPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<TaskListViewModel>(this);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TaskListViewModel vm)
            await vm.LoadTasksAsync().ConfigureAwait(false);
    }

    async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?TaskId={Guid.Empty}").ConfigureAwait(false);
    }

    async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject b && b.BindingContext is TaskItem t)
            await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?TaskId={t.Id}").ConfigureAwait(false);
    }
}
