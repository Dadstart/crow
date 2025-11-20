namespace Dadstart.Labs.Crow.Views;

using Dadstart.Labs.Crow.ViewModels;

public partial class StartupPage : ContentPage
{
    readonly StartupViewModel _viewModel;

    public StartupPage(StartupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.InitializeCommand.CanExecute(null))
        {
            _ = _viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}

