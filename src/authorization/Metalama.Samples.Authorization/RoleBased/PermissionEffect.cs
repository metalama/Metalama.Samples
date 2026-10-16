namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// The effect of a role-permission rule.
/// </summary>
public enum PermissionEffect
{
    /// <summary>
    /// The rule has no effect. It is the default value, so an uninitialized rule never allows anything.
    /// </summary>
    None,

    /// <summary>
    /// Members of the role have the permission, unless another rule denies it.
    /// </summary>
    Allow,

    /// <summary>
    /// Members of the role do not have the permission, even if another rule allows it.
    /// </summary>
    Deny
}
