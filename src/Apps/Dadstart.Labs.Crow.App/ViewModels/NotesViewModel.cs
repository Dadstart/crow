namespace Dadstart.Labs.Crow.ViewModels;

using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Services.Security;

public partial class NotesViewModel : ObservableObject
{
    readonly IVaultApiClient _apiClient;
    readonly VaultSessionState _sessionState;

    public NotesViewModel(IVaultApiClient apiClient, VaultSessionState sessionState)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        AddQuickNoteCommand = new AsyncRelayCommand(AddQuickNoteAsync);
    }

    public ObservableCollection<SecureNote> Notes { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand AddQuickNoteCommand { get; }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    string? searchQuery;

    [ObservableProperty]
    string? statusMessage;

    async Task RefreshAsync()
    {
        if (!_sessionState.IsAuthenticated)
        {
            StatusMessage = "Unlock required.";
            return;
        }

        try
        {
            IsRefreshing = true;
            var token = _sessionState.RequireToken();
            var results = await _apiClient.GetNotesAsync(token, SearchQuery);
            UpdateCollection(Notes, results);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    async Task AddQuickNoteAsync()
    {
        if (!_sessionState.IsAuthenticated)
        {
            StatusMessage = "Unlock required.";
            return;
        }

        try
        {
            var token = _sessionState.RequireToken();
            var mutation = new NoteMutation
            {
                Title = $"Note {DateTimeOffset.Now:t}",
                RichTextBody = "Secure note placeholder",
                IsPinned = false
            };

            await _apiClient.UpsertNoteAsync(token, mutation);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    static void UpdateCollection<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}

