using Crow.Models;
using Crow.ViewModels;

namespace Crow.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
        PageViewModel.AttachWhenReady<SearchViewModel>(this);
    }

    void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        if (BindingContext is SearchViewModel vm && vm.SearchCommand.CanExecute(null))
            vm.SearchCommand.Execute(null);
    }

    async void OnResultSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not SearchResultItem item)
            return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        if (item.Kind == SearchResultKind.Task)
            await Shell.Current.GoToAsync($"{nameof(TaskDetailPage)}?TaskId={item.Id}").ConfigureAwait(false);
        else
            await Shell.Current.GoToAsync($"{nameof(NoteDetailPage)}?NoteId={item.Id}").ConfigureAwait(false);
    }
}
