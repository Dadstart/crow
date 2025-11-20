namespace Dadstart.Labs.Crow.Views;

using Dadstart.Labs.Crow.ViewModels;

public partial class NotesPage : ContentPage
{
    readonly NotesViewModel _viewModel;

    public NotesPage(NotesViewModel viewModel)
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

