using Crow.Services;

namespace Crow;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = ThemeService.GetAppThemeForStartup();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}