namespace Dadstart.Labs.Crow.Views;

using Dadstart.Labs.Crow.ViewModels;

public partial class PasswordsPage : ContentPage
{
    readonly PasswordsViewModel _viewModel;

    public PasswordsPage(PasswordsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.RefreshCommand.ExecuteAsync(null);
    }
}

