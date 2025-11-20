using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.App.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private const string BaseUrl = "http://localhost:5064/api";

    public ApiService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        UpdateAuthHeader();
    }

    private void UpdateAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (_authService.IsAuthenticated && !string.IsNullOrEmpty(_authService.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.Token);
        }
    }

    // Notes
    public async Task<List<Note>> GetNotesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Note>>("notes");
        return response ?? [];
    }

    public async Task<Note?> GetNoteAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Note>($"notes/{id}");
    }

    public async Task<Note> CreateNoteAsync(CreateNoteDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("notes", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Note>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Note?> UpdateNoteAsync(Guid id, UpdateNoteDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"notes/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Note>();
    }

    public async Task<bool> DeleteNoteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"notes/{id}");
        return response.IsSuccessStatusCode;
    }

    // Passwords
    public async Task<List<PasswordResponseDto>> GetPasswordsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<PasswordResponseDto>>("passwords");
        return response ?? [];
    }

    public async Task<PasswordResponseDto?> GetPasswordAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<PasswordResponseDto>($"passwords/{id}");
    }

    public async Task<PasswordResponseDto> CreatePasswordAsync(CreatePasswordDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("passwords", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PasswordResponseDto>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<PasswordResponseDto?> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"passwords/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PasswordResponseDto>();
    }

    public async Task<bool> DeletePasswordAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"passwords/{id}");
        return response.IsSuccessStatusCode;
    }

    // Reminders
    public async Task<List<Reminder>> GetRemindersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Reminder>>("reminders");
        return response ?? [];
    }

    public async Task<Reminder?> GetReminderAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Reminder>($"reminders/{id}");
    }

    public async Task<Reminder> CreateReminderAsync(CreateReminderDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("reminders", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Reminder>() ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<Reminder?> UpdateReminderAsync(Guid id, UpdateReminderDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"reminders/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Reminder>();
    }

    public async Task<bool> DeleteReminderAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"reminders/{id}");
        return response.IsSuccessStatusCode;
    }
}

