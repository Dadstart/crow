using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dadstart.Labs.Crow.Api.IntegrationTests;

public class CrowWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestSecretKeyForIntegrationTestsAtLeast32CharactersLong" },
                { "Jwt:Issuer", "CrowApi" },
                { "Jwt:Audience", "CrowApp" },
                { "Jwt:ExpirationMinutes", "1440" },
                { "Encryption:MasterKey", "TestMasterKeyForIntegrationTests" }
            };
            
            config.AddInMemoryCollection(testConfig);
        });

        builder.UseEnvironment("Testing");
    }
}

