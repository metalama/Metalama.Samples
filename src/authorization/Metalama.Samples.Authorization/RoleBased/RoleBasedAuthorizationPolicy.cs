namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// A role-based <see cref="IAuthorizationPolicy"/>. Rules allow or deny a permission to the members of a role on
/// all objects of a type (including derived types). A subject has a permission on an object when at least one rule
/// allows it and no rule denies it.
/// </summary>
/// <remarks>
/// Rules are typically loaded from a database. Only objects implementing <see cref="IRoleBasedSecurable"/>
/// can be secured; any permission on another object is denied.
/// </remarks>
public sealed class RoleBasedAuthorizationPolicy : IAuthorizationPolicy
{
    private readonly Dictionary<Type, List<Rule>> _rules = [];

    /// <summary>
    /// Allows members of <paramref name="role"/> to have <paramref name="permission"/> on objects of type
    /// <typeparamref name="T"/>. Use <c>object</c> for all types.
    /// </summary>
    public RoleBasedAuthorizationPolicy Allow<T>( Permission permission, Role role )
        => this.AddRule( typeof(T), new Rule( permission, role, PermissionEffect.Allow ) );

    /// <summary>
    /// Denies <paramref name="permission"/> on objects of type <typeparamref name="T"/> to members of
    /// <paramref name="role"/>, overriding any rule that allows it.
    /// </summary>
    public RoleBasedAuthorizationPolicy Deny<T>( Permission permission, Role role )
        => this.AddRule( typeof(T), new Rule( permission, role, PermissionEffect.Deny ) );

    /// <inheritdoc />
    public bool IsGranted( ISubject subject, Permission permission, object securable )
    {
        if ( securable is not IRoleBasedSecurable roleBasedSecurable )
        {
            return false;
        }

        var allowed = false;

        for ( var type = securable.GetType(); type != null; type = type.BaseType )
        {
            if ( !this._rules.TryGetValue( type, out var rules ) )
            {
                continue;
            }

            foreach ( var rule in rules )
            {
                if ( rule.Permission != permission || !roleBasedSecurable.HasRole( subject, rule.Role ) )
                {
                    continue;
                }

                if ( rule.Effect == PermissionEffect.Deny )
                {
                    return false;
                }

                allowed = true;
            }
        }

        return allowed;
    }

    private RoleBasedAuthorizationPolicy AddRule( Type type, Rule rule )
    {
        if ( !this._rules.TryGetValue( type, out var rules ) )
        {
            rules = [];
            this._rules.Add( type, rules );
        }

        rules.Add( rule );

        return this;
    }

    private sealed record Rule( Permission Permission, Role Role, PermissionEffect Effect );
}
