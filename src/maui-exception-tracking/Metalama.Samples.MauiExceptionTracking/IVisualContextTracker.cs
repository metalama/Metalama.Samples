namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Provides ambient context tracking for the current visual element.
/// The context flows correctly through async/await calls using <see cref="AsyncLocal{T}"/>
/// and is scoped to the lifetime of a <c>using</c> block.
/// </summary>
/// <remarks>
/// <para>
/// This interface is implemented by <see cref="VisualContextTracker"/> and registered
/// as a singleton via <see cref="ExceptionTrackingExtensions.UseExceptionTracking"/>.
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// using var scope = visualContextTracker.Push(button);
/// // Current is now 'button'
/// await SomeAsyncOperation(); // Context flows through await
/// // Current is still 'button'
/// // When scope is disposed, previous context is restored
/// </code>
/// </para>
/// </remarks>
public interface IVisualContextTracker
{
    /// <summary>
    /// Gets the current visual element in context, or null if none is set or it has been collected.
    /// </summary>
    VisualElement? Current { get; }

    /// <summary>
    /// Sets the current visual context and returns a disposable that will restore the previous context.
    /// </summary>
    /// <param name="element">The visual element to set as current context.</param>
    /// <returns>An IDisposable that restores the previous context when disposed.</returns>
    IDisposable Push(VisualElement? element);
}
