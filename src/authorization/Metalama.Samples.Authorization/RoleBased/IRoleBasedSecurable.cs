namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// An object secured by <see cref="RoleBasedAuthorizationPolicy"/>: it can tell whether a subject has a role on it.
/// </summary>
public interface IRoleBasedSecurable : ISecurable
{
    /// <summary>
    /// Gets the object from which this object inherits role assignments, or <c>null</c>.
    /// </summary>
    IRoleBasedSecurable? SecurityParent { get; }

    /// <summary>
    /// Determines whether <paramref name="subject"/> has <paramref name="role"/> on this object.
    /// </summary>
    /// <returns><c>true</c> if the subject has the role on this object or on one of its security ancestors,
    /// otherwise <c>false</c>.</returns>
    bool HasRole( ISubject subject, Role role );
}
