using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models.Dtos;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CrowWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly CrowWebApplicationFactory Factory;

    protected IntegrationTestBase(CrowWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var registerDto = new RegisterDto(username, $"test{Guid.NewGuid()}@example.com", "password123");
        
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register user: {response.StatusCode} - {content}");
        }
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
        {
            throw new InvalidOperationException("Failed to get authentication token");
        }

        var authenticatedClient = Factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
        return authenticatedClient;
    }
}

