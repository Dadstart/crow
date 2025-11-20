namespace Dadstart.Labs.Crow.App;

using System;
using CommunityToolkit.Mvvm.Messaging;
using Dadstart.Labs.Crow.Messages;
using Dadstart.Labs.Crow.Server.Hosting;
using Dadstart.Labs.Crow.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

public partial class App : Application
{
    readonly IServiceProvider _services;
    readonly IMessenger _messenger;
    readonly AppShell _shell;
    readonly InProcessVaultServer _server;
    Window? _window;

    public App(IServiceProvider services, IMessenger messenger, AppShell shell, InProcessVaultServer server)
    {
        InitializeComponent();
        _services = services;
        _messenger = messenger;
        _shell = shell;
        _server = server;

        _messenger.Register<VaultUnlockedMessage>(this, (_, message) => OnUnlocked());
        _messenger.Register<VaultLockedMessage>(this, (_, message) => OnLocked());

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    StartupPage CreateStartupPage()
        => _services.GetRequiredService<StartupPage>();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _window = new Window(new NavigationPage(CreateStartupPage()));
        return _window;
    }

    void OnUnlocked()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_window is not null)
            {
                _window.Page = _shell;
            }
        });
    }

    void OnLocked()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_window is not null)
            {
                _window.Page = new NavigationPage(CreateStartupPage());
            }
        });
    }

    void OnProcessExit(object? sender, EventArgs e)
    {
        _messenger.UnregisterAll(this);
        _server.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}