namespace Metalama.Samples.Authorization;

/// <summary>
/// A security subject: a user or a service on whose behalf code runs.
/// </summary>
public interface ISubject
{
    /// <summary>
    /// Gets the name of the subject, used in exception messages and audit records.
    /// </summary>
    string Name { get; }
}
