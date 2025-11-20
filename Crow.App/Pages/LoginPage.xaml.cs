using Dadstart.Labs.Crow.App.ViewModels;

namespace Dadstart.Labs.Crow.App.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageViewModel _viewModel;

    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LoginPageViewModel.IsRegisterMode))
            {
                LoginLabel.IsVisible = !_viewModel.IsRegisterMode;
                RegisterLabel.IsVisible = _viewModel.IsRegisterMode;
                EmailEntry.IsVisible = _viewModel.IsRegisterMode;
                LoginButton.IsVisible = !_viewModel.IsRegisterMode;
                RegisterButton.IsVisible = _viewModel.IsRegisterMode;
                SwitchToRegisterButton.IsVisible = !_viewModel.IsRegisterMode;
                SwitchToLoginButton.IsVisible = _viewModel.IsRegisterMode;
            }
            else if (e.PropertyName == nameof(LoginPageViewModel.ErrorMessage))
            {
                ErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ErrorMessage);
            }
        };
    }
}

