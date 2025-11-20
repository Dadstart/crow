namespace Dadstart.Labs.Crow.ViewModels;

using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Services.Security;

public partial class PasswordsViewModel : ObservableObject
{
    readonly IVaultApiClient _apiClient;
    readonly VaultSessionState _sessionState;

    public PasswordsViewModel(IVaultApiClient apiClient, VaultSessionState sessionState)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        AddCredentialCommand = new AsyncRelayCommand(AddCredentialAsync);
    }

    public ObservableCollection<PasswordEntry> Passwords { get; } = [];

    public IAsyncRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand AddCredentialCommand { get; }

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
            var results = await _apiClient.GetPasswordsAsync(token, SearchQuery);
            UpdateCollection(Passwords, results);
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

    async Task AddCredentialAsync()
    {
        if (!_sessionState.IsAuthenticated)
        {
            StatusMessage = "Unlock required.";
            return;
        }

        try
        {
            var token = _sessionState.RequireToken();
            var mutation = new PasswordMutation
            {
                Title = $"Login {DateTimeOffset.Now:t}",
                Username = "user@example.com",
                Secret = Guid.NewGuid().ToString("N")[..12],
                Notes = "Generated credential",
                IsPinned = false
            };

            await _apiClient.UpsertPasswordAsync(token, mutation);
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

