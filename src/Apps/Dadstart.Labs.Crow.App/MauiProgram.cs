using CommunityToolkit.Mvvm.Messaging;
using Dadstart.Labs.Crow.Abstractions;
using Dadstart.Labs.Crow.Notifications;
using Dadstart.Labs.Crow.Server.Hosting;
using Dadstart.Labs.Crow.Services.Api;
using Dadstart.Labs.Crow.Services.Notifications;
using Dadstart.Labs.Crow.Services.Security;
using Dadstart.Labs.Crow.ViewModels;
using Dadstart.Labs.Crow.Views;
using Microsoft.Extensions.Logging;

using CrowApp = Dadstart.Labs.Crow.App.App;

namespace Dadstart.Labs.Crow;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<CrowApp>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

        builder.Services.AddSingleton<InProcessVaultServer>(_ => InProcessVaultServer.StartAsync().GetAwaiter().GetResult());
        builder.Services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<InProcessVaultServer>().HttpClient;
            if (client.BaseAddress is null)
            {
                client.BaseAddress = new Uri("http://localhost");
            }

            return client;
        });

        builder.Services.AddSingleton<IVaultApiClient, VaultApiClient>();
        builder.Services.AddSingleton<VaultSessionState>();
        builder.Services.AddSingleton<IDeviceSecurityService, DeviceSecurityService>();
        builder.Services.AddSingleton<IReminderNotificationScheduler, LocalReminderNotificationScheduler>();

        builder.Services.AddTransient<StartupViewModel>();
        builder.Services.AddTransient<NotesViewModel>();
        builder.Services.AddTransient<PasswordsViewModel>();
        builder.Services.AddTransient<RemindersViewModel>();

        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddTransient<NotesPage>();
        builder.Services.AddTransient<PasswordsPage>();
        builder.Services.AddTransient<RemindersPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
