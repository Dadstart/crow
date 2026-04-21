using Crow.Views;

namespace Crow;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TaskDetailPage), typeof(TaskDetailPage));
        Routing.RegisterRoute(nameof(NoteDetailPage), typeof(NoteDetailPage));
    }
}
