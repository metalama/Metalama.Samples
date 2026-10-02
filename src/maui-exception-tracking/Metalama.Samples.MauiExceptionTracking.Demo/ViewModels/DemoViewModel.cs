using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Metalama.Samples.MauiExceptionTracking.Demo.ViewModels;

public partial class DemoViewModel : ObservableObject
{
    private readonly ExceptionTracker _exceptionTracker;

    [ObservableProperty]
    private string _name = "Test";

    [ObservableProperty]
    private int _counter;

    public ObservableCollection<ExceptionReport> Exceptions { get; } = new();

    public DemoViewModel(ExceptionTracker exceptionTracker)
    {
        this._exceptionTracker = exceptionTracker;

        // Subscribe to exception reports
        this._exceptionTracker.ExceptionReported += this.OnExceptionReported;

        // Add any existing exceptions
        foreach (var exception in this._exceptionTracker.RecentExceptions)
        {
            this.Exceptions.Insert(0, exception);
        }
    }

    private void OnExceptionReported(object? sender, ExceptionReport report)
    {
        // Ensure we're on the UI thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            this.Exceptions.Insert(0, report);
        });
    }

    [RelayCommand]
    private void ThrowSync()
    {
        throw new InvalidOperationException("Sync command exception");
    }

    [RelayCommand]
    private async Task ThrowDelayedAsync()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("Async command exception");
    }

    [RelayCommand]
    private void IncrementCounter()
    {
        this.Counter++;
    }

    [RelayCommand]
    private void ClearExceptions()
    {
        this.Exceptions.Clear();
        this._exceptionTracker.Clear();
    }

    [RelayCommand]
    private void RefreshVisualContext()
    {
        foreach (var report in this.Exceptions)
        {
            report.RefreshVisualContext();
        }

        // Trigger UI update by notifying about the collection
        this.OnPropertyChanged(nameof(this.Exceptions));
    }
}
