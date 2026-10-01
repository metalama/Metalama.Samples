namespace Metalama.Samples.MauiExceptionTracking.Demo;

/// <summary>
/// Represents a recorded exception with its visual context information.
/// </summary>
/// <remarks>
/// <para>
/// This class captures:
/// <list type="bullet">
/// <item>The exception itself</item>
/// <item>A weak reference to the source visual element (if provided)</item>
/// <item>The element's type name, automation ID, and visual tree path</item>
/// <item>The timestamp when the exception was reported</item>
/// <item>Optional additional context string</item>
/// </list>
/// </para>
/// <para>
/// The visual element is stored as a <see cref="WeakReference{T}"/> to avoid
/// preventing garbage collection of disposed UI elements. Use <see cref="RefreshVisualContext"/>
/// to update the cached element information if the element is still alive.
/// </para>
/// </remarks>
public sealed class ExceptionReport
{
    private readonly WeakReference<VisualElement>? _sourceElement;

    /// <summary>
    /// Gets the exception that was reported.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the type name of the source visual element (e.g., "Button", "Label").
    /// May be null if no source element was provided.
    /// </summary>
    public string? ElementType { get; private set; }

    /// <summary>
    /// Gets the name of the source element (AutomationId or StyleId).
    /// May be null if the element has no identifier.
    /// </summary>
    public string? ElementName { get; private set; }

    /// <summary>
    /// Gets the full visual tree path from the root to the source element.
    /// Format: "ContentPage > VerticalStackLayout > Button(MyButton)"
    /// </summary>
    public string? VisualTreePath { get; private set; }

    /// <summary>
    /// Gets any additional context information provided when the exception was reported.
    /// </summary>
    public string? AdditionalContext { get; }

    /// <summary>
    /// Gets the timestamp when the exception was reported.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the source visual element if it's still alive.
    /// </summary>
    public VisualElement? SourceElement
    {
        get
        {
            if (this._sourceElement != null && this._sourceElement.TryGetTarget(out var element))
            {
                return element;
            }
            return null;
        }
    }

    public ExceptionReport(
        Exception exception,
        VisualElement? sourceElement,
        string? additionalContext)
    {
        this.Exception = exception;
        this._sourceElement = sourceElement != null ? new WeakReference<VisualElement>(sourceElement) : null;
        this.AdditionalContext = additionalContext;
        this.Timestamp = DateTime.Now;

        // Capture initial visual context data
        this.RefreshVisualContext();
    }

    /// <summary>
    /// Refreshes the visual context data from the source element if it's still alive.
    /// Call this to get updated element name, type, and visual tree path.
    /// </summary>
    public void RefreshVisualContext()
    {
        var element = this.SourceElement;
        if (element != null)
        {
            this.ElementType = element.GetType().Name;
            this.ElementName = GetElementName(element);
            this.VisualTreePath = BuildVisualTreePath(element);
        }
    }

    public override string ToString()
    {
        var elementInfo = this.ElementName != null
            ? $"{this.ElementType} ({this.ElementName})"
            : this.ElementType ?? "Unknown";

        return $"[{this.Timestamp:HH:mm:ss}] {this.Exception.GetType().Name}: {this.Exception.Message} - Source: {elementInfo}";
    }

    private static string? GetElementName(VisualElement element)
    {
        if (!string.IsNullOrEmpty(element.AutomationId))
        {
            return element.AutomationId;
        }

        if (!string.IsNullOrEmpty(element.StyleId))
        {
            return element.StyleId;
        }

        return null;
    }

    private static string BuildVisualTreePath(VisualElement element)
    {
        var parts = new List<string>();
        Element? current = element;

        while (current != null)
        {
            var name = current switch
            {
                VisualElement ve when !string.IsNullOrEmpty(ve.AutomationId) =>
                    $"{current.GetType().Name}({ve.AutomationId})",
                VisualElement ve when !string.IsNullOrEmpty(ve.StyleId) =>
                    $"{current.GetType().Name}({ve.StyleId})",
                _ => current.GetType().Name
            };

            parts.Add(name);
            current = current.Parent;
        }

        parts.Reverse();
        return string.Join(" > ", parts);
    }
}
