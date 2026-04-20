using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Crow.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
	int _count;

	public string CounterButtonText =>
		_count == 0 ? "Click me" : _count == 1 ? $"Clicked {_count} time" : $"Clicked {_count} times";

	public ICommand IncrementCommand { get; }

	public MainViewModel()
	{
		IncrementCommand = new Command(Increment);
	}

	void Increment()
	{
		_count++;
		OnPropertyChanged(nameof(CounterButtonText));
		SemanticScreenReader.Announce(CounterButtonText);
	}
}
