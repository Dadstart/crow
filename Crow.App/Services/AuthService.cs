using System.Net.Http.Json;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Dadstart.Labs.Crow.Models.Dtos;
using Microsoft.Maui.Storage;

namespace Dadstart.Labs.Crow.App.Services;

public class AuthService : ObservableObject, IAuthService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5064/api";
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UsernameKey = "auth_username";
    private const string UserIdKey = "auth_userid";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        LoadStoredAuth();
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Username { get; private set; }
    public Guid? UserId { get; private set; }

    private void LoadStoredAuth()
    {
        try
        {
            Token = SecureStorage.GetAsync(TokenKey).Result;
            RefreshToken = SecureStorage.GetAsync(RefreshTokenKey).Result;
            Username = SecureStorage.GetAsync(UsernameKey).Result;
            var userIdString = SecureStorage.GetAsync(UserIdKey).Result;
            if (Guid.TryParse(userIdString, out var userId))
                UserId = userId;
        }
        catch
        {
            Token = null;
            RefreshToken = null;
            Username = null;
            UserId = null;
        }
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var dto = new RegisterDto(username, email, password);
            var response = await _httpClient.PostAsJsonAsync("auth/register", dto);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse == null)
                return false;

            await StoreAuth(authResponse);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var dto = new LoginDto(username, password);
            var response = await _httpClient.PostAsJsonAsync("auth/login", dto);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse == null)
                return false;

            await StoreAuth(authResponse);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        Token = null;
        RefreshToken = null;
        Username = null;
        UserId = null;

        try
        {
            SecureStorage.Remove(TokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(UsernameKey);
            SecureStorage.Remove(UserIdKey);
        }
        catch
        {
        }

        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(Token));
        OnPropertyChanged(nameof(RefreshToken));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(UserId));

        await Task.CompletedTask;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(RefreshToken))
            return false;

        try
        {
            var dto = new RefreshTokenDto(RefreshToken);
            var response = await _httpClient.PostAsJsonAsync("auth/refresh", dto);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse == null)
                return false;

            await StoreAuth(authResponse);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task StoreAuth(AuthResponseDto authResponse)
    {
        Token = authResponse.Token;
        RefreshToken = authResponse.RefreshToken;
        Username = authResponse.Username;
        UserId = authResponse.UserId;

        try
        {
            await SecureStorage.SetAsync(TokenKey, authResponse.Token);
            await SecureStorage.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
            await SecureStorage.SetAsync(UsernameKey, authResponse.Username);
            await SecureStorage.SetAsync(UserIdKey, authResponse.UserId.ToString());
        }
        catch
        {
        }
        
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(Token));
        OnPropertyChanged(nameof(RefreshToken));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(UserId));
    }
}

