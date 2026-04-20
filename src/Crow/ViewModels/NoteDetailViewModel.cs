using System.Globalization;
using System.Windows.Input;
using Crow.Models;
using Crow.Repositories;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class NoteDetailViewModel : BaseViewModel
{
    readonly NoteRepository _noteRepository;

    NoteItem? _currentNote;
    string _tagsText = "";
    bool _isNewNote;

    public NoteDetailViewModel(NoteRepository noteRepository)
    {
        _noteRepository = noteRepository;

        LoadNoteCommand = new Command<Guid>(
            async id => await LoadNoteAsync(id).ConfigureAwait(false),
            id => id != Guid.Empty);
        AddNoteCommand = new Command<NoteItem>(
            async note => await AddNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
        UpdateNoteCommand = new Command<NoteItem>(
            async note => await UpdateNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
        DeleteNoteCommand = new Command<NoteItem>(
            async note => await DeleteNoteAsync(note).ConfigureAwait(false),
            note => note is not null);
        SaveCommand = new Command(async () => await SaveAsync().ConfigureAwait(false), () => CurrentNote != null);
        DeleteCommand = new Command(async () => await DeleteCurrentAsync().ConfigureAwait(false), () => CurrentNote != null && !IsNewNote);
    }

    public NoteItem? CurrentNote
    {
        get => _currentNote;
        private set
        {
            if (ReferenceEquals(_currentNote, value))
                return;
            _currentNote = value;
            OnPropertyChanged();
            if (SaveCommand is Command saveCmd)
                saveCmd.ChangeCanExecute();
            if (DeleteCommand is Command delCmd)
                delCmd.ChangeCanExecute();
        }
    }

    public bool IsNewNote
    {
        get => _isNewNote;
        private set
        {
            if (_isNewNote == value)
                return;
            _isNewNote = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageTitle));
            if (DeleteCommand is Command deleteCmd)
                deleteCmd.ChangeCanExecute();
        }
    }

    public string PageTitle => IsNewNote ? "New note" : "Edit note";

    public string TagsText
    {
        get => _tagsText;
        set
        {
            if (_tagsText == value)
                return;
            _tagsText = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadNoteCommand { get; }

    public ICommand AddNoteCommand { get; }

    public ICommand UpdateNoteCommand { get; }

    public ICommand DeleteNoteCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public void BeginNewNote()
    {
        IsNewNote = true;
        CurrentNote = new NoteItem
        {
            Title = "",
            Content = "",
            Tags = [],
        };
        TagsText = "";
    }

    public async Task LoadNoteAsync(Guid id)
    {
        IsNewNote = false;
        CurrentNote = await _noteRepository.GetByIdAsync(id).ConfigureAwait(false);
        SyncEditorsFromNote();
    }

    public async Task SaveAsync()
    {
        if (CurrentNote == null)
            return;

        ApplyEditorsToNote();

        if (IsNewNote)
        {
            await AddNoteAsync(CurrentNote).ConfigureAwait(false);
            IsNewNote = false;
        }
        else
            await UpdateNoteAsync(CurrentNote).ConfigureAwait(false);

        SyncEditorsFromNote();
    }

    public async Task AddNoteAsync(NoteItem note)
    {
        await _noteRepository.AddAsync(note).ConfigureAwait(false);
        CurrentNote = await _noteRepository.GetByIdAsync(note.Id).ConfigureAwait(false);
    }

    public async Task UpdateNoteAsync(NoteItem note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        await _noteRepository.UpdateAsync(note).ConfigureAwait(false);
        CurrentNote = await _noteRepository.GetByIdAsync(note.Id).ConfigureAwait(false);
    }

    public async Task DeleteNoteAsync(NoteItem note)
    {
        await _noteRepository.DeleteAsync(note.Id).ConfigureAwait(false);
        CurrentNote = null;
        TagsText = "";
    }

    async Task DeleteCurrentAsync()
    {
        if (CurrentNote == null)
            return;
        await DeleteNoteAsync(CurrentNote).ConfigureAwait(false);
    }

    void ApplyEditorsToNote()
    {
        if (CurrentNote == null)
            return;

        CurrentNote.Tags = TagsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    void SyncEditorsFromNote()
    {
        if (CurrentNote == null)
            return;

        TagsText = string.Join(", ", CurrentNote.Tags);
    }
}
