using Microsoft.Extensions.DependencyInjection;

namespace Crow.Views;

internal static class PageViewModel
{
    internal static void AttachWhenReady<TViewModel>(ContentPage page, Action? onViewModelBound = null)
        where TViewModel : class
    {
        void HandlerChanged(object? sender, EventArgs e)
        {
            if (page.Handler?.MauiContext?.Services is not IServiceProvider services)
                return;

            page.HandlerChanged -= HandlerChanged;
            page.BindingContext = services.GetRequiredService<TViewModel>();
            onViewModelBound?.Invoke();
        }

        page.HandlerChanged += HandlerChanged;
    }
}
