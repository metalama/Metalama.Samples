using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// A department of the <see cref="Company"/>. Invoices, customers and budgets belong to a department
/// and inherit its role assignments.
/// </summary>
public sealed class Department( Company company, string code, string name ) : Entity
{
    // Used by Entity Framework, which then sets the backing fields.
    private Department() : this( null!, null!, null! ) { }

    public Company Company { get; } = company;

    public string Code { get; } = code;

    // Init-only: the application cannot rename a department, whatever the rules of the policy.
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the number of the last invoice created in the department. The member is <c>internal</c>, but the project
    /// fabric secures non-public members too, so other code of the assembly cannot read it without permission.
    /// </summary>
    internal int LastInvoiceNumber { get; private set; }

    protected override IRoleBasedSecurable? SecurityParent => this.Company;

    /// <summary>
    /// Creates a draft invoice owned by the current user. The user must be allowed to create invoices
    /// in this department, and to read <paramref name="customer"/>. The caller adds the invoice to the database.
    /// </summary>
    [RequiresPermission( DemoPermissions.CreateInvoice )]
    public Invoice CreateInvoice( [RequiresPermission( PermissionNames.Read )] Customer customer )
    {
        var creator = SecurityContext.Current?.Subject as User;

        // The permission has been checked: build the object in a trusted scope,
        // where setting the owner does not require the ChangeOwnership permission.
        using ( SecurityContext.RunAsSystem() )
        {
            this.LastInvoiceNumber++;

            return new Invoice( this, customer, $"{this.Code}-{this.LastInvoiceNumber:0000}", creator ) { Owner = creator };
        }
    }

    public override string ToString() => this.Name;
}
