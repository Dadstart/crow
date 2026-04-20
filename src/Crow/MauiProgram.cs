using Crow.Repositories;
using Crow.Services;
using Crow.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Crow;

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

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "crow.db");
        builder.Services.AddSingleton(_ => new DatabaseService(dbPath));
        builder.Services.AddSingleton<TaskRepository>();
        builder.Services.AddSingleton<NoteRepository>();
        builder.Services.AddSingleton<SearchService>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<TaskListViewModel>();
        builder.Services.AddTransient<TaskDetailViewModel>();
        builder.Services.AddTransient<NoteListViewModel>();
        builder.Services.AddTransient<NoteDetailViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.GetRequiredService<DatabaseService>().Initialize();
        return app;
    }
}
