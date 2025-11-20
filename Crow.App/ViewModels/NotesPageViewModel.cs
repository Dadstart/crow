using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Crow.App.Services;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.ViewModels;

public partial class NotesPageViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<Note> _notes = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _tags = string.Empty;

    [ObservableProperty]
    private Note? _selectedNote;

    public NotesPageViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadNotesAsync()
    {
        IsLoading = true;
        try
        {
            Notes = await _apiService.GetNotesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreateNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return;

        var tagsList = string.IsNullOrWhiteSpace(Tags)
            ? []
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var dto = new CreateNoteDto(Title, Content, tagsList);
        await _apiService.CreateNoteAsync(dto);

        Title = string.Empty;
        Content = string.Empty;
        Tags = string.Empty;

        await LoadNotesAsync();
    }

    [RelayCommand]
    public async Task UpdateNoteAsync(Note note)
    {
        if (note == null)
            return;

        var tagsList = string.IsNullOrWhiteSpace(Tags)
            ? []
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var dto = new UpdateNoteDto(Title, Content, tagsList);
        await _apiService.UpdateNoteAsync(note.Id, dto);

        Title = string.Empty;
        Content = string.Empty;
        Tags = string.Empty;
        SelectedNote = null;

        await LoadNotesAsync();
    }

    [RelayCommand]
    public async Task DeleteNoteAsync(Note note)
    {
        if (note == null)
            return;

        await _apiService.DeleteNoteAsync(note.Id);
        await LoadNotesAsync();
    }

    [RelayCommand]
    public void SelectNote(Note note)
    {
        SelectedNote = note;
        Title = note.Title;
        Content = note.Content;
        Tags = string.Join(", ", note.Tags);
    }

    [RelayCommand]
    public void ClearSelection()
    {
        SelectedNote = null;
        Title = string.Empty;
        Content = string.Empty;
        Tags = string.Empty;
    }
}

