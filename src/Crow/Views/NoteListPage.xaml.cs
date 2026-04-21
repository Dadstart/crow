using Crow.Models;
using Crow.ViewModels;

namespace Crow.Views;

public partial class NoteListPage : ContentPage
{
    public NoteListPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<NoteListViewModel>(this);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NoteListViewModel vm)
            await vm.LoadNotesAsync().ConfigureAwait(false);
    }

    async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(NoteDetailPage)}?NoteId={Guid.Empty}").ConfigureAwait(false);
    }

    async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject b && b.BindingContext is NoteItem n)
            await Shell.Current.GoToAsync($"{nameof(NoteDetailPage)}?NoteId={n.Id}").ConfigureAwait(false);
    }
}
