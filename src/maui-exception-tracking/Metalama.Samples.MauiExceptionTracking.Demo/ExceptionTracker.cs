using System.Collections.Concurrent;
using Metalama.Samples.MauiExceptionTracking;

namespace Metalama.Samples.MauiExceptionTracking.Demo;

/// <summary>
/// Demo implementation of <see cref="IExceptionReporter"/> that stores exceptions in memory
/// and raises events for UI notification.
/// </summary>
public sealed class ExceptionTracker : IExceptionReporter
{
    private const int _maxStoredExceptions = 100;
    private readonly ConcurrentQueue<ExceptionReport> _recentExceptions = new();

    /// <summary>
    /// Event raised when an exception is reported.
    /// </summary>
    public event EventHandler<ExceptionReport>? ExceptionReported;

    /// <summary>
    /// Gets the recent exception reports.
    /// </summary>
    public IEnumerable<ExceptionReport> RecentExceptions => this._recentExceptions.ToArray();

    /// <inheritdoc />
    public void Report(Exception ex, VisualElement? source, string? additionalContext = null)
    {
        var report = new ExceptionReport(ex, source, additionalContext);

        this._recentExceptions.Enqueue(report);

        // Trim old exceptions
        while (this._recentExceptions.Count > _maxStoredExceptions && this._recentExceptions.TryDequeue(out _))
        {
        }

        this.ExceptionReported?.Invoke(this, report);
    }

    /// <summary>
    /// Clears all stored exception reports.
    /// </summary>
    public void Clear()
    {
        while (this._recentExceptions.TryDequeue(out _))
        {
        }
    }
}
