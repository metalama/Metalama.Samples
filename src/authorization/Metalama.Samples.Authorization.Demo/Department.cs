using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// A department of the <see cref="Company"/>. Invoices, customers and budgets belong to a department
/// and inherit its role assignments.
/// </summary>
public sealed class Department( Company company, string code, string name ) : Entity
{
    // Used by Entity Framework, which creates entities in a system scope.
    private Department() : this( null!, null!, null! ) { }

    public Company Company { get; } = company;

    public string Code { get; } = code;

    // Init-only: the application cannot rename a department, whatever the rules of the policy.
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the number of the last invoice created in the department. The member is <c>internal</c>, but properties
    /// are secured whatever their accessibility, so other code of the assembly cannot read it without permission.
    /// Only the creation of an invoice advances the counter, so the setter requires <c>CreateInvoice</c>.
    /// </summary>
    [RequiresPermission( PermissionNames.Read, DemoPermissions.CreateInvoice )]
    internal int LastInvoiceNumber { get; private set; }

    protected override IRoleBasedSecurable? SecurityParent => this.Company;

    /// <summary>
    /// Creates a draft invoice owned by the current user. The user must be allowed to create invoices
    /// in this department, and to read <paramref name="customer"/>. The caller adds the invoice to the database.
    /// </summary>
    /// <remarks>
    /// The method reads the invoice counter of the department, so it grants <c>Read</c> on the department: the numbering
    /// must not depend on a rule of the policy. Writing the counter requires <c>CreateInvoice</c>, which the method
    /// has already checked. The constructor of <see cref="Invoice"/> grants the permissions that it
    /// needs on the new invoice.
    /// </remarks>
    [RequiresPermission( DemoPermissions.CreateInvoice )]
    [GrantsPermissions( PermissionNames.Read )]
    public Invoice CreateInvoice( [RequiresPermission( PermissionNames.Read )] Customer customer )
    {
        var creator = SecurityContext.Current?.Subject as User;

        this.LastInvoiceNumber++;

        return new Invoice( this, customer, $"{this.Code}-{this.LastInvoiceNumber:0000}", creator );
    }

    public override string ToString() => this.Name;
}
