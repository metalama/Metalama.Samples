using Microsoft.Extensions.Logging;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Writes every denied access to the application log, as a security audit trail.
/// </summary>
/// <remarks>
/// Besides the denials, the auditor records the granted accesses that change who can do what: role management and
/// changes of ownership.
/// </remarks>
public sealed partial class LoggingAccessAuditor( ILogger<LoggingAccessAuditor> logger ) : IAccessAuditor
{
    public void OnAccessDenied( AccessDeniedException exception )
        => this.LogAccessDenied( exception.Subject.Name, exception.Permission.Name, exception.Member, exception.Securable );

    public bool AuditsGrantsOf( Permission permission )
        => permission.Name is PermissionNames.ManageRoles or PermissionNames.ChangeOwnership;

    public void OnAccessGranted( AccessRecord record )
        => this.LogAccessGranted( record.Subject.Name, record.Permission.Name, record.Member, record.Securable );

    [LoggerMessage( Level = LogLevel.Warning, Message = "{Subject} was denied {Permission} on {Member} of {Securable}." )]
    private partial void LogAccessDenied( string subject, string permission, string member, string securable );

    [LoggerMessage( Level = LogLevel.Information, Message = "{Subject} was granted {Permission} on {Member} of {Securable}." )]
    private partial void LogAccessGranted( string subject, string permission, string member, string securable );
}
