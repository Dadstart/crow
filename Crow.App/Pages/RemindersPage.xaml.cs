using Dadstart.Labs.Crow.App.ViewModels;
using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.App.Pages;

public partial class RemindersPage : ContentPage
{
    private readonly RemindersPageViewModel _viewModel;

    public RemindersPage(RemindersPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        Loaded += async (s, e) => await viewModel.LoadRemindersCommand.ExecuteAsync(null);
    }

    private void OnReminderSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Reminder reminder)
        {
            _viewModel.SelectReminderCommand.Execute(reminder);
        }
    }

    private void OnReminderToggled(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Reminder reminder)
        {
            _viewModel.ToggleReminderCompletionCommand.Execute(reminder);
        }
    }
}

