using System.Collections.ObjectModel;
using System.Windows.Input;
using Crow.Models;
using Crow.Repositories;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class NoteListViewModel : BaseViewModel
{
    readonly NoteRepository _noteRepository;

    ObservableCollection<NoteItem> _notes = [];

    public NoteListViewModel(NoteRepository noteRepository)
    {
        _noteRepository = noteRepository;

        LoadNotesCommand = new Command(async () => await LoadNotesAsync().ConfigureAwait(false));
        AddNoteCommand = new Command<NoteItem>(
            async note => await AddNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
        UpdateNoteCommand = new Command<NoteItem>(
            async note => await UpdateNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
        DeleteNoteCommand = new Command<NoteItem>(
            async note => await DeleteNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
    }

    public ObservableCollection<NoteItem> Notes
    {
        get => _notes;
        private set
        {
            if (ReferenceEquals(_notes, value))
                return;
            _notes = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadNotesCommand { get; }

    public ICommand AddNoteCommand { get; }

    public ICommand UpdateNoteCommand { get; }

    public ICommand DeleteNoteCommand { get; }

    public async Task LoadNotesAsync()
    {
        var items = await _noteRepository.GetAllAsync().ConfigureAwait(false);
        Notes = new ObservableCollection<NoteItem>(items);
    }

    public async Task AddNoteAsync(NoteItem note)
    {
        await _noteRepository.AddAsync(note).ConfigureAwait(false);
        await LoadNotesAsync().ConfigureAwait(false);
    }

    public async Task UpdateNoteAsync(NoteItem note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        await _noteRepository.UpdateAsync(note).ConfigureAwait(false);
        await LoadNotesAsync().ConfigureAwait(false);
    }

    public async Task DeleteNoteAsync(NoteItem note)
    {
        await _noteRepository.DeleteAsync(note.Id).ConfigureAwait(false);
        await LoadNotesAsync().ConfigureAwait(false);
    }
}
