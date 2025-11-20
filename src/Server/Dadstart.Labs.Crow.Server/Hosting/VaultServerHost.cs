namespace Dadstart.Labs.Crow.Server.Hosting;

using Dadstart.Labs.Crow.Server.Endpoints;
using Dadstart.Labs.Crow.Server.Extensions;
using Microsoft.AspNetCore.TestHost;

public static class VaultServerHost
{
    public static WebApplication BuildApplication()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddVaultServer();

        var app = builder.Build();
        app.MapVaultEndpoints();

        return app;
    }
}

