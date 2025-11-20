namespace Dadstart.Labs.Crow.Server.Extensions;

using Dadstart.Labs.Crow.Server.Repositories;
using Dadstart.Labs.Crow.Server.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVaultServer(this IServiceCollection services)
    {
        services.AddSingleton<IVaultRepository, InMemoryVaultRepository>();
        services.AddSingleton<VaultSecretsStore>();
        services.AddSingleton<IVaultSessionService, InMemoryVaultSessionService>();
        services.AddSingleton<IVaultSecurityService, InMemoryVaultSecurityService>();

        return services;
    }
}

