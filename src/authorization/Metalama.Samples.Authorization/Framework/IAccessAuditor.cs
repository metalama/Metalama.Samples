namespace Metalama.Samples.Authorization;

/// <summary>
/// Receives a notification each time an access is denied, before the exception is thrown.
/// A typical implementation writes an audit log.
/// </summary>
public interface IAccessAuditor
{
    void OnAccessDenied( AccessDeniedException exception );
}
