namespace Metalama.Samples.Authorization;

/// <summary>
/// The kind of access being authorized.
/// </summary>
public enum AccessKind
{
    /// <summary>
    /// Executing a method, or passing an argument to a method.
    /// </summary>
    Execute,

    /// <summary>
    /// Reading a field or property.
    /// </summary>
    Read,

    /// <summary>
    /// Writing a field or property.
    /// </summary>
    Write
}
