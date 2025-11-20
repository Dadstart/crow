using Dadstart.Labs.Crow.App.ViewModels;

namespace Dadstart.Labs.Crow.App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

