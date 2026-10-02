namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Extension methods for setting up visual context tracking in a MAUI application.
/// </summary>
public static class VisualContextTrackingExtensions
{
    /// <summary>
    /// Configures the MAUI application to use visual context tracking.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <returns>The MauiAppBuilder instance for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method in your <c>MauiProgram.CreateMauiApp()</c> to register the
    /// visual context tracking service:
    /// <code>
    /// var builder = MauiApp.CreateBuilder();
    /// builder
    ///     .UseMauiApp&lt;App&gt;()
    ///     .UseVisualContextTracking()
    ///     .Services.AddSingleton&lt;IExceptionReporter, MyExceptionReporter&gt;();
    /// </code>
    /// </para>
    /// <para>
    /// This registers <see cref="IVisualContextTracker"/> as a singleton, implemented by
    /// <see cref="VisualContextTracker"/>.
    /// </para>
    /// <para>
    /// You must also register your own implementation of <see cref="IExceptionReporter"/>
    /// to handle reported exceptions (e.g., logging, displaying to user, sending to telemetry).
    /// </para>
    /// </remarks>
    public static MauiAppBuilder UseVisualContextTracking(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IVisualContextTracker, VisualContextTracker>();

        return builder;
    }
}
