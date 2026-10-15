using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// A role-based <see cref="IAuthorizationPolicy"/>. Rules allow or deny a permission to the members of a role on
/// all objects of a class (including derived classes). A subject has a permission on an object when at least one
/// rule allows it and no rule denies it.
/// </summary>
/// <remarks>
/// <para>A policy is immutable and can be used by concurrent requests. Create it with a
/// <see cref="RoleBasedAuthorizationPolicyBuilder"/>. To change the rules, build a new policy and replace the
/// old one, for instance in the object that provides the policy to the requests.</para>
/// <para>Only objects implementing <see cref="IRoleBasedSecurable"/> can be secured; any permission on another
/// object is denied.</para>
/// </remarks>
public sealed class RoleBasedAuthorizationPolicy : IAuthorizationPolicy
{
    private readonly FrozenDictionary<Type, ImmutableArray<RoleBasedAuthorizationPolicyBuilder.Rule>> _rules;

    internal RoleBasedAuthorizationPolicy( FrozenDictionary<Type, ImmutableArray<RoleBasedAuthorizationPolicyBuilder.Rule>> rules )
    {
        this._rules = rules;
    }

    /// <inheritdoc />
    public bool IsGranted( ISubject subject, Permission permission, ISecurable securable )
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

                switch ( rule.Effect )
                {
                    case PermissionEffect.Deny:
                        return false;

                    case PermissionEffect.Allow:
                        allowed = true;

                        break;
                }
            }
        }

        return allowed;
    }
}

/// <summary>
/// Collects the rules of a <see cref="RoleBasedAuthorizationPolicy"/>. A builder is not thread-safe;
/// the policy that it builds is.
/// </summary>
public sealed class RoleBasedAuthorizationPolicyBuilder
{
    private readonly Dictionary<Type, List<Rule>> _rules = [];

    /// <summary>
    /// Allows members of <paramref name="role"/> to have <paramref name="permission"/> on objects of class
    /// <typeparamref name="T"/>. Use <c>object</c> for all classes.
    /// </summary>
    public RoleBasedAuthorizationPolicyBuilder Allow<T>( Permission permission, Role role )
        => this.Add( typeof(T), permission, role, PermissionEffect.Allow );

    /// <summary>
    /// Denies <paramref name="permission"/> on objects of class <typeparamref name="T"/> to members of
    /// <paramref name="role"/>, overriding any rule that allows it.
    /// </summary>
    public RoleBasedAuthorizationPolicyBuilder Deny<T>( Permission permission, Role role )
        => this.Add( typeof(T), permission, role, PermissionEffect.Deny );

    /// <summary>
    /// Adds a rule for objects of class <paramref name="type"/>. Use this method when the rules are data, for instance
    /// rules loaded from a database.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="type"/> is an interface, or <paramref name="effect"/> is
    /// <see cref="PermissionEffect.None"/>.</exception>
    public RoleBasedAuthorizationPolicyBuilder Add( Type type, Permission permission, Role role, PermissionEffect effect )
    {
        if ( effect is not (PermissionEffect.Allow or PermissionEffect.Deny) )
        {
            throw new ArgumentOutOfRangeException( nameof(effect), effect, "A rule must allow or deny." );
        }

        return this.AddRule( type, new Rule( permission, role, effect ) );
    }

    public RoleBasedAuthorizationPolicy Build()
        => new( this._rules.ToFrozenDictionary( pair => pair.Key, pair => ImmutableArray.CreateRange( pair.Value ) ) );

    private RoleBasedAuthorizationPolicyBuilder AddRule( Type type, Rule rule )
    {
        // The policy walks the base classes of the securable, never its interfaces, so a rule on an interface
        // would silently never match.
        if ( type.IsInterface )
        {
            throw new ArgumentException( $"Rules apply to classes. '{type.Name}' is an interface.", nameof(type) );
        }

        if ( !this._rules.TryGetValue( type, out var rules ) )
        {
            rules = [];
            this._rules.Add( type, rules );
        }

        rules.Add( rule );

        return this;
    }

    internal sealed record Rule( Permission Permission, Role Role, PermissionEffect Effect );
}
