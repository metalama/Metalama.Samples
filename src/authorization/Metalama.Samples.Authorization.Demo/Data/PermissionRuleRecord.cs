using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Data;

/// <summary>
/// A role-permission rule of the policy, as it is stored in the database. The class, the permission and the role are
/// stored by name, so that an administrator can read and edit the rules. <see cref="DatabaseAuthorizationPolicy"/>
/// loads them into a <see cref="RoleBasedAuthorizationPolicy"/>.
/// </summary>
public sealed class PermissionRuleRecord
{
    /// <summary>
    /// The value of <see cref="ObjectType"/> for a rule that applies to the objects of every class.
    /// </summary>
    public const string AnyType = "*";

    // Used by Entity Framework.
    private PermissionRuleRecord()
    {
        this.ObjectType = null!;
        this.Permission = null!;
        this.Role = null!;
    }

    public PermissionRuleRecord( string objectType, string permission, string role, PermissionEffect effect )
    {
        this.ObjectType = objectType;
        this.Permission = permission;
        this.Role = role;
        this.Effect = effect;
    }

    public int Id { get; private set; }

    /// <summary>
    /// Gets the name of the class of the objects to which the rule applies, for instance <c>Invoice</c>, or
    /// <see cref="AnyType"/>.
    /// </summary>
    public string ObjectType { get; private set; }

    public string Permission { get; private set; }

    public string Role { get; private set; }

    public PermissionEffect Effect { get; private set; }

    public override string ToString() => $"{this.Effect} {this.Permission} on {this.ObjectType} to {this.Role}";
}
