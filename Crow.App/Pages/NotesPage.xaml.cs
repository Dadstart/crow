using Dadstart.Labs.Crow.App.ViewModels;
using Dadstart.Labs.Crow.Models;

namespace Dadstart.Labs.Crow.App.Pages;

public partial class NotesPage : ContentPage
{
    private readonly NotesPageViewModel _viewModel;

    public NotesPage(NotesPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        Loaded += async (s, e) => await viewModel.LoadNotesCommand.ExecuteAsync(null);
    }

    private void OnNoteSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Note note)
        {
            _viewModel.SelectNoteCommand.Execute(note);
        }
    }
}

