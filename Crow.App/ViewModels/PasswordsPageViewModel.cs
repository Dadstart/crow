using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Crow.App.Services;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.ViewModels;

public partial class PasswordsPageViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private List<PasswordResponseDto> _passwords = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private PasswordResponseDto? _selectedPassword;

    public PasswordsPageViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadPasswordsAsync()
    {
        IsLoading = true;
        try
        {
            Passwords = await _apiService.GetPasswordsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreatePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return;

        var dto = new CreatePasswordDto(Title, Username, Password, string.IsNullOrWhiteSpace(Url) ? null : Url, string.IsNullOrWhiteSpace(Notes) ? null : Notes);
        await _apiService.CreatePasswordAsync(dto);

        Title = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Url = string.Empty;
        Notes = string.Empty;

        await LoadPasswordsAsync();
    }

    [RelayCommand]
    public async Task UpdatePasswordAsync(PasswordResponseDto password)
    {
        if (password == null)
            return;

        var dto = new UpdatePasswordDto(Title, Username, Password, string.IsNullOrWhiteSpace(Url) ? null : Url, string.IsNullOrWhiteSpace(Notes) ? null : Notes);
        await _apiService.UpdatePasswordAsync(password.Id, dto);

        Title = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Url = string.Empty;
        Notes = string.Empty;
        SelectedPassword = null;

        await LoadPasswordsAsync();
    }

    [RelayCommand]
    public async Task DeletePasswordAsync(PasswordResponseDto password)
    {
        if (password == null)
            return;

        await _apiService.DeletePasswordAsync(password.Id);
        await LoadPasswordsAsync();
    }

    [RelayCommand]
    public void SelectPassword(PasswordResponseDto password)
    {
        SelectedPassword = password;
        Title = password.Title;
        Username = password.Username;
        Password = password.DecryptedPassword ?? string.Empty;
        Url = password.Url ?? string.Empty;
        Notes = password.Notes ?? string.Empty;
    }

    [RelayCommand]
    public void ClearSelection()
    {
        SelectedPassword = null;
        Title = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Url = string.Empty;
        Notes = string.Empty;
    }
}

