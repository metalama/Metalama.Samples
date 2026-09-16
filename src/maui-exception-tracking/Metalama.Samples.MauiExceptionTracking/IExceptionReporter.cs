namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Service for reporting exceptions with visual context information.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface to handle exceptions reported by the exception tracking infrastructure.
/// Register your implementation as a singleton in your MAUI app's DI container.
/// </para>
/// <para>
/// This service is used by:
/// <list type="bullet">
/// <item><see cref="SafeCommandExtension"/> - Reports command execution exceptions</item>
/// <item><see cref="SafeBindingExtension"/> - Reports value converter exceptions</item>
/// <item><see cref="TrackExceptionsAttribute"/> - Reports exceptions from instrumented methods</item>
/// <item><see cref="ReportExceptionsAttribute"/> - Reports exceptions from decorated methods</item>
/// </list>
/// </para>
/// </remarks>
public interface IExceptionReporter
{
    /// <summary>
    /// Reports an exception with optional visual context.
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    /// <param name="source">The visual element that caused the exception, if known.</param>
    /// <param name="additionalContext">Additional context information.</param>
    void Report(Exception ex, VisualElement? source, string? additionalContext = null);
}
