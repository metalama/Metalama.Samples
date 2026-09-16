namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// Extension methods for accessing MAUI's dependency injection service provider.
/// </summary>
/// <remarks>
/// <para>
/// MAUI's service provider is not always readily accessible, especially during XAML parsing
/// or before handlers are attached. This class provides helper methods that try multiple
/// fallback strategies to obtain the service provider.
/// </para>
/// </remarks>
public static class MauiServiceProviderExtensions
{
    /// <summary>
    /// Gets the MAUI service provider from a visual element, with fallback to application-level providers.
    /// </summary>
    /// <param name="element">The visual element to get services from.</param>
    /// <returns>The service provider, or null if not available.</returns>
    /// <remarks>
    /// <para>
    /// Resolution order:
    /// <list type="number">
    /// <item>Element's handler's MauiContext.Services (if handler is attached)</item>
    /// <item>Application.Current's handler's MauiContext.Services</item>
    /// <item>IPlatformApplication.Current.Services (platform-specific, earliest availability)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: The element's handler is only available after the element is added to the visual tree.
    /// During XAML parsing, the fallbacks are used.
    /// </para>
    /// </remarks>
    public static IServiceProvider? GetMauiServiceProvider(this VisualElement? element)
    {
        // Try to get services from the element's handler
        if (element?.Handler?.MauiContext?.Services is { } services)
        {
            return services;
        }

        // Fall back to Application.Current
        if (Application.Current?.Handler?.MauiContext?.Services is { } appServices)
        {
            return appServices;
        }

        // Fall back to IPlatformApplication (available earlier in lifecycle)
#if WINDOWS || ANDROID || IOS || MACCATALYST
        if (IPlatformApplication.Current?.Services is { } platformServices)
        {
            return platformServices;
        }
#endif

        return null;
    }

}
