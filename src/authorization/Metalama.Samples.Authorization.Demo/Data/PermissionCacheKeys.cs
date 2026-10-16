using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// The configuration of <see cref="CachingAuthorizationPolicy"/> for the entities of the demo.
/// </summary>
public static class PermissionCacheKeys
{
    /// <summary>
    /// Returns the key of an entity or a user: its class and its database key. An object that has not been saved has
    /// no key yet, so the decisions on it are not cached.
    /// </summary>
    public static object? GetKey( object value )
        => value switch
        {
            Entity { Id: > 0 } entity => (entity.GetType(), entity.Id),
            User { Id: > 0 } user => (typeof(User), user.Id),
            _ => null
        };

    /// <summary>
    /// Returns the security parents of an object. The role-based policy reads their role assignments, so a change of a
    /// parent invalidates the decisions on its children.
    /// </summary>
    public static IEnumerable<ISecurable> GetSecurityParents( ISecurable securable )
    {
        for ( var parent = (securable as IRoleBasedSecurable)?.SecurityParent; parent != null; parent = parent.SecurityParent )
        {
            yield return parent;
        }
    }
}
