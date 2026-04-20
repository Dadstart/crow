using Crow.ViewModels;

namespace Crow.Views;

public partial class ThemeSettingsPage : ContentPage
{
    public ThemeSettingsPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<ThemeSettingsViewModel>(this);
    }
}
