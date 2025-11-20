using System.Net;
using System.Net.Http.Json;
using Dadstart.Labs.Crow.Models.Dtos;
using Xunit;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class PasswordsIntegrationTests : IntegrationTestBase
{
    public PasswordsIntegrationTests(CrowWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePassword_ShouldEncryptAndStorePassword()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreatePasswordDto("Test Site", "testuser", "MySecretPassword123", "https://example.com", "Notes");

        var response = await client.PostAsJsonAsync("/api/passwords", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var passwordResponse = await response.Content.ReadFromJsonAsync<PasswordResponseDto>();
        Assert.NotNull(passwordResponse);
        Assert.Equal("MySecretPassword123", passwordResponse.DecryptedPassword);
        Assert.Equal("Test Site", passwordResponse.Title);
        Assert.Equal("testuser", passwordResponse.Username);
    }

    [Fact]
    public async Task GetAllPasswords_ShouldReturnDecryptedPasswords()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto1 = new CreatePasswordDto("Site 1", "user1", "password1", null, null);
        var dto2 = new CreatePasswordDto("Site 2", "user2", "password2", null, null);

        var create1 = await client.PostAsJsonAsync("/api/passwords", dto1);
        create1.EnsureSuccessStatusCode();
        var create2 = await client.PostAsJsonAsync("/api/passwords", dto2);
        create2.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/passwords");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var passwords = await response.Content.ReadFromJsonAsync<List<PasswordResponseDto>>();
        Assert.NotNull(passwords);
        Assert.True(passwords.Count >= 2);
        Assert.All(passwords, p => Assert.NotNull(p.DecryptedPassword));
    }

    [Fact]
    public async Task UpdatePassword_ShouldUpdateAndReencrypt()
    {
        var client = await GetAuthenticatedClientAsync();
        var createDto = new CreatePasswordDto("Original", "user", "oldpassword", null, null);
        var createResponse = await client.PostAsJsonAsync("/api/passwords", createDto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PasswordResponseDto>();

        var updateDto = new UpdatePasswordDto("Updated", "newuser", "newpassword", "https://newurl.com", "New notes");
        var updateResponse = await client.PutAsJsonAsync($"/api/passwords/{created!.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<PasswordResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
        Assert.Equal("newuser", updated.Username);
        Assert.Equal("newpassword", updated.DecryptedPassword);
    }

    [Fact]
    public async Task DeletePassword_ShouldRemovePassword()
    {
        var client = await GetAuthenticatedClientAsync();
        var dto = new CreatePasswordDto("To Delete", "user", "password", null, null);
        var createResponse = await client.PostAsJsonAsync("/api/passwords", dto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PasswordResponseDto>();

        var deleteResponse = await client.DeleteAsync($"/api/passwords/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/passwords/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

