namespace Dadstart.Labs.Crow.Tests;

using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using Dadstart.Labs.Crow.Constants;
using Dadstart.Labs.Crow.Contracts;
using Dadstart.Labs.Crow.Models;
using Dadstart.Labs.Crow.Security;
using Dadstart.Labs.Crow.Server.Hosting;

public class VaultServerTests : IAsyncLifetime
{
    readonly InProcessVaultServer _server;
    readonly HttpClient _client;
    readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public VaultServerTests()
    {
        _server = InProcessVaultServer.StartAsync().GetAwaiter().GetResult();
        _client = _server.HttpClient;
    }

    [Fact]
    public async Task UnlockAndCreateNote_Succeeds()
    {
        await InitializeVaultAsync();

        var unlockResponse = await PostAsync<UnlockResponse>(VaultApiRoutes.Unlock, new UnlockRequest
        {
            Method = AuthenticationMethod.Pin,
            Pin = "123456",
            DeviceId = "test-device"
        });

        Assert.False(string.IsNullOrWhiteSpace(unlockResponse.SessionToken));

        var mutation = new NoteMutation
        {
            Title = "Integration Note",
            RichTextBody = "Server integration test body",
            IsPinned = true
        };

        var note = await AuthorizedPostAsync<SecureNote>(VaultApiRoutes.Notes, mutation, unlockResponse.SessionToken);

        Assert.Equal("Integration Note", note.Title);

        var notes = await AuthorizedGetAsync<List<SecureNote>>(VaultApiRoutes.Notes, unlockResponse.SessionToken);
        Assert.Single(notes);
    }

    async Task InitializeVaultAsync()
    {
        var state = await GetAsync<VaultSetupState>(VaultApiRoutes.SetupState);
        if (state.IsConfigured)
        {
            return;
        }

        await PostAsync<VaultSetupState>(VaultApiRoutes.Setup, new VaultSetupRequest
        {
            Pin = "123456",
            EnableBiometric = true
        });
    }

    async Task<T> GetAsync<T>(string path)
    {
        var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
        return payload!;
    }

    async Task<T> AuthorizedGetAsync<T>(string path, string sessionToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation(VaultApiRoutes.SessionHeader, sessionToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
        return payload!;
    }

    async Task<T> PostAsync<T>(string path, object payload)
    {
        var response = await _client.PostAsJsonAsync(path, payload, _serializerOptions);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
        return result!;
    }

    async Task<T> AuthorizedPostAsync<T>(string path, object payload, string sessionToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload, options: _serializerOptions)
        };
        request.Headers.TryAddWithoutValidation(VaultApiRoutes.SessionHeader, sessionToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
        return result!;
    }

    public Task InitializeAsync()
        => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _server.DisposeAsync();
    }
}

