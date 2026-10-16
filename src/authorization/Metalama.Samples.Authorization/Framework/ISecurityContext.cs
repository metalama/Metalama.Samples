namespace Metalama.Samples.Authorization;

/// <summary>
/// The ambient security settings: who is calling, which policy applies, and who is notified of denials.
/// Exposed by <see cref="SecurityContext.Current"/>.
/// </summary>
public interface ISecurityContext
{
    /// <summary>
    /// Gets the current subject, typically the signed-in user.
    /// </summary>
    ISubject Subject { get; }

    /// <summary>
    /// Gets the authorization policy. To combine several policies, use a policy that delegates to them.
    /// </summary>
    IAuthorizationPolicy Policy { get; }

    /// <summary>
    /// Gets the optional auditor notified of every denied access.
    /// </summary>
    IAccessAuditor? Auditor { get; }
}

/// <summary>
/// A plain implementation of <see cref="ISecurityContext"/>.
/// </summary>
public sealed record UserSecurityContext(
    ISubject Subject,
    IAuthorizationPolicy Policy,
    IAccessAuditor? Auditor = null ) : ISecurityContext;
