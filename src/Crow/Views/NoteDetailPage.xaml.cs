using Crow.ViewModels;

namespace Crow.Views;

[QueryProperty(nameof(NoteIdQuery), "NoteId")]
public partial class NoteDetailPage : ContentPage
{
    string? _pendingNoteId;

    public string NoteIdQuery
    {
        set
        {
            _pendingNoteId = value;
            TryApplyQuery();
        }
    }

    public NoteDetailPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<NoteDetailViewModel>(this, TryApplyQuery);
    }

    void TryApplyQuery()
    {
        if (BindingContext is not NoteDetailViewModel vm)
            return;
        if (_pendingNoteId == null)
            return;

        if (!Guid.TryParse(_pendingNoteId, out var id) || id == Guid.Empty)
            vm.BeginNewNote();
        else
            _ = vm.LoadNoteAsync(id);

        _pendingNoteId = null;
    }

    async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not NoteDetailViewModel vm)
            return;

        await vm.SaveAsync().ConfigureAwait(false);
        await Shell.Current.GoToAsync("..").ConfigureAwait(false);
    }

    async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not NoteDetailViewModel vm || vm.IsNewNote || vm.CurrentNote == null)
            return;

        await vm.DeleteNoteAsync(vm.CurrentNote).ConfigureAwait(false);
        await Shell.Current.GoToAsync("..").ConfigureAwait(false);
    }
}
