using Metalama.Samples.Authorization.Demo.Data;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Base class of all business objects. The <see cref="ApplyDefaultPermissionsAttribute"/> aspect is inherited,
/// so every derived class gets permission checks on its members, configured by <see cref="AuthorizationOptions"/>.
/// The owner and the role assignments are stored with Entity Framework by <see cref="EfRoleBasedSecurable"/>.
/// </summary>
[ApplyDefaultPermissions]
public abstract class Entity : EfRoleBasedSecurable, IHasSecurityLabel
{
    /// <summary>
    /// Gets the label used in denial messages and audit records, for instance <c>Invoice #12</c>.
    /// It contains no protected data, so it is not secured.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string SecurityLabel => $"{this.GetType().Name} #{this.Id}";

    /// <summary>
    /// Gets the database key. It identifies the object and is not confidential.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public int Id { get; private set; }
}
