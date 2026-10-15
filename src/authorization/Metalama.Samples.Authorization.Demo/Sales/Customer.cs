using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// A customer, managed by a <see cref="Department"/>.
/// </summary>
/// <remarks>
/// Commercial terms such as the credit limit are a management decision, so the type-level option makes
/// <see cref="DemoPermissions.Approve"/> the default permission to write any property. Contact details
/// override this back to <see cref="PermissionNames.Write"/>, and the tax ID has its own explicit permissions.
/// </remarks>
[AuthorizationOptions( DefaultWritePermission = DemoPermissions.Approve )]
public sealed class Customer( Department department, string name ) : Entity
{
    // Used by Entity Framework, which creates entities in a system scope.
    private Customer() : this( null!, null! ) { }

    // The department changes only through MoveTo, which checks the destination.
    public Department Department { get; private set; } = department;

    [AuthorizationOptions( DefaultWritePermission = PermissionNames.Write )]
    public string Name { get; set; } = name;

    [AuthorizationOptions( DefaultWritePermission = PermissionNames.Write )]
    public string? Email { get; set; }

    [RequiresPermission( DemoPermissions.ReadSensitive, PermissionNames.Write )]
    public string? TaxId { get; set; }

    public decimal CreditLimit { get; set; }

    public int PaymentTermDays { get; set; } = 30;

    protected override IRoleBasedSecurable? SecurityParent => this.Department;

    /// <summary>
    /// Moves the customer to another department. The user needs <c>ChangeOwnership</c> on the customer
    /// and on the destination.
    /// </summary>
    [RequiresPermission( PermissionNames.ChangeOwnership )]
    public void MoveTo( [RequiresPermission( PermissionNames.ChangeOwnership )] Department target ) => this.Department = target;

    public override string ToString() => this.Name;
}
