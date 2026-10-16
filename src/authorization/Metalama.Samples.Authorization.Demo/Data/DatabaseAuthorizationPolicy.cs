using Metalama.Samples.Authorization.RoleBased;
using Microsoft.EntityFrameworkCore;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// The authorization policy of the application, whose rules are stored in the database (see
/// <see cref="PermissionRuleRecord"/>). The rules are loaded into an immutable <see cref="RoleBasedAuthorizationPolicy"/>.
/// After an administrator changes the rules, <see cref="ReloadAsync"/> replaces this policy, and the change applies
/// to the next checks of every request.
/// </summary>
/// <remarks>
/// Before the rules are loaded, every check is denied: a policy that could not be loaded must not allow anything.
/// </remarks>
public sealed class DatabaseAuthorizationPolicy : IAuthorizationPolicy
{
    // The classes to which a rule can apply, by the name that the database stores.
    private static readonly Dictionary<string, Type> _objectTypes =
        typeof(Entity).Assembly.GetTypes()
            .Where( type => type.IsSubclassOf( typeof(Entity) ) )
            .ToDictionary( type => type.Name );

    // The permissions that the code checks: the constants of PermissionNames and DemoPermissions.
    private static readonly HashSet<string> _permissions =
        new[] { typeof(PermissionNames), typeof(DemoPermissions) }
            .SelectMany( type => type.GetFields() )
            .Where( field => field.IsLiteral && field.FieldType == typeof(string) )
            .Select( field => (string) field.GetRawConstantValue()! )
            .ToHashSet();

    private volatile RoleBasedAuthorizationPolicy? _policy;

    /// <summary>
    /// Occurs after the rules have been reloaded, for instance to invalidate a cache of decisions.
    /// </summary>
    public event Action? Reloaded;

    public bool IsGranted( ISubject subject, Permission permission, ISecurable securable )
        => this._policy?.IsGranted( subject, permission, securable ) ?? false;

    /// <summary>
    /// Loads the rules from the database and replaces the current policy.
    /// </summary>
    /// <exception cref="InvalidOperationException">A rule names an unknown class, permission or role. The current
    /// policy is not changed.</exception>
    public async Task ReloadAsync( InvoicingDbContext db, CancellationToken cancellationToken = default )
    {
        var rules = await db.PermissionRules.AsNoTracking().OrderBy( r => r.Id ).ToListAsync( cancellationToken );
        var builder = new RoleBasedAuthorizationPolicyBuilder();

        foreach ( var rule in rules )
        {
            builder.Add( GetObjectType( rule ), GetPermission( rule ), GetRole( rule ), rule.Effect );
        }

        this._policy = builder.Build();
        this.Reloaded?.Invoke();
    }

    // A rule on an unknown class, permission or role is a configuration error. Loading it could silently remove a
    // Deny rule: a mistyped name matches no check, or a role that nobody has.
    private static Permission GetPermission( PermissionRuleRecord rule )
        => _permissions.Contains( rule.Permission )
            ? rule.Permission
            : throw new InvalidOperationException( $"The rule '{rule}' names the unknown permission '{rule.Permission}'." );

    private static Role GetRole( PermissionRuleRecord rule )
        => DemoRoles.FromName( rule.Role ) ?? throw new InvalidOperationException( $"The rule '{rule}' names the unknown role '{rule.Role}'." );

    private static Type GetObjectType( PermissionRuleRecord rule )
        => rule.ObjectType == PermissionRuleRecord.AnyType
            ? typeof(object)
            : _objectTypes.GetValueOrDefault( rule.ObjectType )
              ?? throw new InvalidOperationException( $"The rule '{rule}' applies to the unknown class '{rule.ObjectType}'." );
}
