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
    /// <exception cref="InvalidOperationException">A rule names an unknown class.</exception>
    public async Task ReloadAsync( InvoicingDbContext db, CancellationToken cancellationToken = default )
    {
        var rules = await db.PermissionRules.AsNoTracking().OrderBy( r => r.Id ).ToListAsync( cancellationToken );
        var builder = new RoleBasedAuthorizationPolicyBuilder();

        foreach ( var rule in rules )
        {
            builder.Add( GetObjectType( rule ), rule.Permission, DemoRoles.FromName( rule.Role ), rule.Effect );
        }

        this._policy = builder.Build();
        this.Reloaded?.Invoke();
    }

    // A rule on an unknown class is a configuration error. Ignoring it could silently remove a Deny rule.
    private static Type GetObjectType( PermissionRuleRecord rule )
        => rule.ObjectType == PermissionRuleRecord.AnyType
            ? typeof(object)
            : _objectTypes.GetValueOrDefault( rule.ObjectType )
              ?? throw new InvalidOperationException( $"The rule '{rule}' applies to the unknown class '{rule.ObjectType}'." );
}
