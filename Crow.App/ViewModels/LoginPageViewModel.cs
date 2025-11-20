using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dadstart.Labs.Crow.App.Services;

namespace Dadstart.Labs.Crow.App.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isRegisterMode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginPageViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }
        catch
        {
            ErrorMessage = "An error occurred during login";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username, email, and password are required";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authService.RegisterAsync(Username, Email, Password);
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Registration failed. Username or email may already exist.";
            }
        }
        catch
        {
            ErrorMessage = "An error occurred during registration";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        ErrorMessage = string.Empty;
    }
}

