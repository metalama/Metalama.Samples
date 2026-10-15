using Microsoft.Extensions.Logging;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Writes every denied access to the application log, as a security audit trail.
/// </summary>
public sealed partial class LoggingAccessAuditor( ILogger<LoggingAccessAuditor> logger ) : IAccessAuditor
{
    public void OnAccessDenied( AccessDeniedException exception )
        => this.LogAccessDenied( exception.Subject.Name, exception.Permission.Name, exception.Member, exception.Securable );

    [LoggerMessage( Level = LogLevel.Warning, Message = "{Subject} was denied {Permission} on {Member} of '{Securable}'." )]
    private partial void LogAccessDenied( string subject, string permission, string member, object securable );
}
