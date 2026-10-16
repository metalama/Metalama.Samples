namespace Metalama.Samples.Authorization;

/// <summary>
/// The state of an object whose initialization has started and not ended (see
/// <see cref="SecurityContext.BeginInitialization"/>).
/// </summary>
internal enum InitializationState
{
    /// <summary>
    /// The constructors are running.
    /// </summary>
    Constructing,

    /// <summary>
    /// The constructors have run. The object initializer may be running.
    /// </summary>
    Constructed
}
