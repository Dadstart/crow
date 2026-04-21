using System.Collections.ObjectModel;
using System.Windows.Input;
using Crow.Models;
using Crow.Services;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class SearchViewModel : BaseViewModel
{
    readonly SearchService _searchService;

    string _queryText = "";
    ObservableCollection<SearchResultItem> _results = [];

    public SearchViewModel(SearchService searchService)
    {
        _searchService = searchService;
        SearchCommand = new Command(async () => await SearchAsync().ConfigureAwait(false));
    }

    public string QueryText
    {
        get => _queryText;
        set
        {
            if (_queryText == value)
                return;
            _queryText = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SearchResultItem> Results
    {
        get => _results;
        private set
        {
            if (ReferenceEquals(_results, value))
                return;
            _results = value;
            OnPropertyChanged();
        }
    }

    public ICommand SearchCommand { get; }

    public async Task SearchAsync()
    {
        var items = await _searchService.SearchAllAsync(QueryText).ConfigureAwait(false);
        Results = new ObservableCollection<SearchResultItem>(items);
    }
}
