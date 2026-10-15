namespace Metalama.Samples.Authorization;

/// <summary>
/// Decides whether a subject has a permission on an object. The framework does not assume any security model:
/// see <see cref="RoleBased.RoleBasedAuthorizationPolicy"/> for a role-based implementation.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Determines whether <paramref name="subject"/> has <paramref name="permission"/> on <paramref name="securable"/>.
    /// </summary>
    /// <remarks>
    /// The framework calls this method in a system scope (see <see cref="SecurityContext.RunAsSystem"/>), so the
    /// policy can read secured members of <paramref name="securable"/> without triggering checks recursively.
    /// </remarks>
    bool IsGranted( ISubject subject, Permission permission, ISecurable securable );
}
