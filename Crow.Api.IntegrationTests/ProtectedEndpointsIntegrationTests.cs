using System.Net;
using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models.Dtos;
using Xunit;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class ProtectedEndpointsIntegrationTests : IClassFixture<CrowWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CrowWebApplicationFactory _factory;

    public ProtectedEndpointsIntegrationTests(CrowWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", "password123");
        
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.EnsureSuccessStatusCode();
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!.Token;
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetNotes_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNotes_ShouldReturnOk_WhenAuthenticated()
    {
        var token = await GetAuthTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);

        var response = await authenticatedClient.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var dto = new CreateNoteDto("Test", "Content", null);
        var response = await _client.PostAsJsonAsync("/api/notes", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_ShouldReturnCreated_WhenAuthenticated()
    {
        var token = await GetAuthTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);
        var dto = new CreateNoteDto("Test Note", "Test Content", ["tag1", "tag2"]);

        var response = await authenticatedClient.PostAsJsonAsync("/api/notes", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<Dadstart.Labs.Crow.Models.Note>();
        Assert.NotNull(note);
        Assert.Equal("Test Note", note.Title);
    }

    [Fact]
    public async Task GetPasswords_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/passwords");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePassword_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var dto = new CreatePasswordDto("Test", "user", "password", null, null);
        var response = await _client.PostAsJsonAsync("/api/passwords", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePassword_ShouldEncryptPassword_WhenAuthenticated()
    {
        var token = await GetAuthTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);
        var dto = new CreatePasswordDto("Test", "user", "plainPassword123", "https://example.com", "Notes");

        var response = await authenticatedClient.PostAsJsonAsync("/api/passwords", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var passwordResponse = await response.Content.ReadFromJsonAsync<PasswordResponseDto>();
        Assert.NotNull(passwordResponse);
        Assert.Equal("plainPassword123", passwordResponse.DecryptedPassword);
        Assert.Equal("Test", passwordResponse.Title);
    }

    [Fact]
    public async Task GetReminders_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/reminders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateReminder_ShouldReturnCreated_WhenAuthenticated()
    {
        var token = await GetAuthTokenAsync();
        var authenticatedClient = CreateAuthenticatedClient(token);
        var dto = new CreateReminderDto("Test Reminder", "Description", TimeProvider.System.GetUtcNow().AddDays(1));

        var response = await authenticatedClient.PostAsJsonAsync("/api/reminders", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var reminder = await response.Content.ReadFromJsonAsync<Dadstart.Labs.Crow.Models.Reminder>();
        Assert.NotNull(reminder);
        Assert.Equal("Test Reminder", reminder.Title);
    }
}

