namespace Dadstart.Labs.Crow.Views;

using Dadstart.Labs.Crow.ViewModels;

public partial class RemindersPage : ContentPage
{
    readonly RemindersViewModel _viewModel;

    public RemindersPage(RemindersViewModel viewModel)
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

