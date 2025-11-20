namespace Dadstart.Labs.Crow.Server.Hosting;

using Dadstart.Labs.Crow.Server.Extensions;
using Dadstart.Labs.Crow.Server.Transport;
using Microsoft.Extensions.DependencyInjection;

public sealed class InProcessVaultServer : IAsyncDisposable
{
    readonly IServiceProvider _services;
    readonly HttpMessageHandler _handler;

    InProcessVaultServer(IServiceProvider services, HttpMessageHandler handler)
    {
        _services = services;
        _handler = handler;
        HttpClient = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("https://crow.vault.local")
        };
    }

    public HttpClient HttpClient { get; }

    public static Task<InProcessVaultServer> StartAsync(CancellationToken cancellationToken = default)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddVaultServer();

        var provider = services.BuildServiceProvider();
        var handler = ActivatorUtilities.CreateInstance<VaultRouterHandler>(provider);

        return Task.FromResult(new InProcessVaultServer(provider, handler));
    }

    public ValueTask DisposeAsync()
    {
        HttpClient.Dispose();
        _handler.Dispose();
        if (_services is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return ValueTask.CompletedTask;
    }
}
