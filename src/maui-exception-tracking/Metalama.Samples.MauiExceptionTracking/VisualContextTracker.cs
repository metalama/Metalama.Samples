namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Provides ambient context tracking for the current visual element.
/// Uses AsyncLocal to flow context through async/await.
/// </summary>
public sealed partial class VisualContextTracker : IVisualContextTracker
{
    private readonly AsyncLocal<VisualElement?> _current = new();

    /// <inheritdoc />
    public VisualElement? Current => this._current.Value;

    /// <inheritdoc />
    public IDisposable Push(VisualElement? element)
    {
        if ( element == null )
        {
            return ContextScope.Empty;
        }
        else
        {
            var previous = this._current.Value;
            this._current.Value = element;
            return new ContextScope( this._current, previous );
        }
    }
}
