using Crow.Models;
using Crow.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Crow.Views;

public partial class TaskListPage : ContentPage
{
    public TaskListPage()
    {
        InitializeComponent();
        HandlerChanged += OnHandlerChanged;
    }

    void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (Handler?.MauiContext?.Services is not IServiceProvider services)
            return;

        HandlerChanged -= OnHandlerChanged;
        BindingContext = services.GetRequiredService<TaskListViewModel>();
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
