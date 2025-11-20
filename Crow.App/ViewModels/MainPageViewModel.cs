using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Crow.App.Services;

namespace Dadstart.Labs.Crow.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    public MainPageViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Username = authService.Username ?? "User";
    }

    [RelayCommand]
    private async Task GoToNotesAsync()
    {
        await Shell.Current.GoToAsync("//NotesPage");
    }

    [RelayCommand]
    private async Task GoToPasswordsAsync()
    {
        await Shell.Current.GoToAsync("//PasswordsPage");
    }

    [RelayCommand]
    private async Task GoToRemindersAsync()
    {
        await Shell.Current.GoToAsync("//RemindersPage");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}

