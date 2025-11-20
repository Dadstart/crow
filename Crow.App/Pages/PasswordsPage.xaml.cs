using Dadstart.Labs.Crow.App.ViewModels;
using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.App.Pages;

public partial class PasswordsPage : ContentPage
{
    private readonly PasswordsPageViewModel _viewModel;

    public PasswordsPage(PasswordsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        Loaded += async (s, e) => await viewModel.LoadPasswordsCommand.ExecuteAsync(null);
    }

    private void OnPasswordSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Password password)
        {
            _viewModel.SelectPasswordCommand.Execute(password);
        }
    }
}

