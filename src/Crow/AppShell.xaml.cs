using Crow.Views;

namespace Crow;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
    }
}
