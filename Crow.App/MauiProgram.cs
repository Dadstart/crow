using Microsoft.Extensions.Logging;
using Dadstart.Labs.Crow.App.Services;
using CommunityToolkit.Mvvm;

namespace Dadstart.Labs.Crow.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IAuthService, Services.AuthService>();
        builder.Services.AddHttpClient<IApiService, ApiService>();

        builder.Services.AddTransient<ViewModels.MainPageViewModel>();
        builder.Services.AddTransient<ViewModels.LoginPageViewModel>();
        builder.Services.AddTransient<ViewModels.NotesPageViewModel>();
        builder.Services.AddTransient<ViewModels.PasswordsPageViewModel>();
        builder.Services.AddTransient<ViewModels.RemindersPageViewModel>();

        builder.Services.AddTransient<Pages.LoginPage>();

        builder.Services.AddTransient<Pages.MainPage>();
        builder.Services.AddTransient<Pages.NotesPage>();
        builder.Services.AddTransient<Pages.PasswordsPage>();
        builder.Services.AddTransient<Pages.RemindersPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
