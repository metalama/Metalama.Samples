namespace Metalama.Samples.Authorization;

/// <summary>
/// Receives access decisions for a security audit trail: every denied access, and the granted accesses
/// that the auditor selects with <see cref="AuditsGrantsOf"/>.
/// </summary>
/// <remarks>
/// The framework calls the auditor outside of any system scope, so an auditor that reads secured data or writes to
/// a secured store does it with the permissions of the current subject.
/// </remarks>
public interface IAccessAuditor
{
    /// <summary>
    /// Called before an <see cref="AccessDeniedException"/> is thrown.
    /// </summary>
    void OnAccessDenied( AccessDeniedException exception );

    /// <summary>
    /// Determines whether the granted accesses that require <paramref name="permission"/> are audited.
    /// Typically, the permissions that change access rights are, such as <see cref="PermissionNames.ManageRoles"/>.
    /// </summary>
    bool AuditsGrantsOf( Permission permission ) => false;

    /// <summary>
    /// Called after an access is granted, for the permissions selected by <see cref="AuditsGrantsOf"/>.
    /// </summary>
    void OnAccessGranted( AccessRecord record ) { }
}
