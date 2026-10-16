using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// An invoice issued by a <see cref="Department"/> to a <see cref="Customer"/>.
/// </summary>
/// <remarks>
/// <para>Permission checks come from three sources: the <see cref="SecurableTypeAttribute"/> aspect
/// inherited from <see cref="Entity"/> (properties need <c>Read</c>/<c>Write</c>), the project fabric (methods
/// without an attribute need <c>Write</c>), and explicit <see cref="RequiresPermissionAttribute"/> attributes.</para>
/// <para>Authorization answers "may this user do this?". Business rules, such as "an approved invoice cannot be
/// edited", are a separate concern and are enforced by the methods themselves, and by
/// <see cref="ISecurableCollectionOwner.OnCollectionChanging"/> for the lines.</para>
/// </remarks>
public sealed class Invoice : Entity, ISecurableCollectionOwner
{
    // Used by Entity Framework, which creates entities in a system scope, so the constructor can set secured properties.
    private Invoice() : this( null!, null!, null!, null ) { }

    /// <summary>
    /// Creates a draft invoice, owned by its creator. <see cref="Demo.Department.CreateInvoice"/> checks that the
    /// creator may create it.
    /// </summary>
    /// <remarks>
    /// The constructor sets secured members of the new invoice, so it grants the permissions that they require on it.
    /// The grant does not apply to any other object.
    /// </remarks>
    [GrantsPermissions( PermissionNames.Write, PermissionNames.ChangeOwnership )]
    internal Invoice( Department department, Customer customer, string number, User? creator )
    {
        this.Department = department;
        this.Customer = customer;
        this.Number = number;
        this.CreatorUser = creator;
        this.Owner = creator;
        this.DueDate = DateOnly.FromDateTime( DateTime.Today ).AddDays( 30 );
    }

    /// <summary>
    /// Gets the invoice number. It is printed on every document and quoted by customers on the phone,
    /// so it is excluded from default securing.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string Number { get; }

    // The customer changes only through ChangeCustomer, which checks the new customer.
    public Customer Customer { get; private set; } = null!;

    /// <summary>
    /// Gets the person who created the invoice. Unlike the owner, the creator never changes.
    /// </summary>
    public ISubject? Creator => this.CreatorUser;

    public DateOnly DueDate { get; set; }

    // The department changes only through TransferTo, which requires ChangeOwnership.
    [RequiresPermission( PermissionNames.Read, PermissionNames.ChangeOwnership )]
    public Department Department { get; private set; }

    // The status, the payments and the void reason change only through the methods of the workflow, which grant
    // ChangeStatus while they run.
    [RequiresPermission( PermissionNames.Read, DemoPermissions.ChangeStatus )]
    public InvoiceStatus Status { get; private set; }

    public IList<InvoiceLine> Lines { get; } = new SecurableList<InvoiceLine>();

    /// <summary>
    /// Gets the internal notes of the invoice. Unlike the lines, notes can change at any status.
    /// </summary>
    public IList<string> Notes { get; } = new SecurableList<string>();

    public decimal Total => this.Lines.Sum( l => l.Amount );

    [RequiresPermission( PermissionNames.Read, DemoPermissions.ChangeStatus )]
    public decimal AmountPaid { get; private set; }

    [RequiresPermission( PermissionNames.Read, DemoPermissions.ChangeStatus )]
    public string? VoidReason { get; private set; }

    protected override IRoleBasedSecurable? SecurityParent => this.Department;

    // Storage of the creator, mapped by Entity Framework.
    private User? CreatorUser { get; set; }

    protected override bool HasComputedRole( ISubject subject, Role role )
        => role == DemoRoles.Creator && subject.Equals( this.CreatorUser );

    /// <summary>
    /// Bills another customer. The user must be able to edit the invoice and to read the new customer.
    /// </summary>
    public void ChangeCustomer( [RequiresPermission( PermissionNames.Read )] Customer customer )
    {
        this.EnsureStatus( InvoiceStatus.Draft );
        this.Customer = customer;
    }

    [RequiresPermission( DemoPermissions.Approve )]
    [GrantsPermissions( DemoPermissions.ChangeStatus )]
    public void Approve()
    {
        this.EnsureStatus( InvoiceStatus.Draft );

        if ( this.Lines.Count == 0 )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} has no line." );
        }

        this.Status = InvoiceStatus.Approved;
    }

    /// <summary>
    /// Sends the approved invoice to the customer through <paramref name="sender"/>.
    /// </summary>
    [RequiresPermission( DemoPermissions.Send )]
    [GrantsPermissions( DemoPermissions.ChangeStatus )]
    public async Task SendAsync( IInvoiceSender sender, CancellationToken cancellationToken = default )
    {
        this.EnsureStatus( InvoiceStatus.Approved );

        await sender.SendAsync( this, cancellationToken );

        this.Status = InvoiceStatus.Sent;
    }

    [RequiresPermission( DemoPermissions.RecordPayment )]
    [GrantsPermissions( DemoPermissions.ChangeStatus )]
    public void RecordPayment( decimal amount )
    {
        this.EnsureStatus( InvoiceStatus.Sent );

        this.AmountPaid += amount;

        if ( this.AmountPaid >= this.Total )
        {
            this.Status = InvoiceStatus.Paid;
        }
    }

    [RequiresPermission( DemoPermissions.Void )]
    [GrantsPermissions( DemoPermissions.ChangeStatus )]
    public void Void( string reason )
    {
        if ( this.Status is InvoiceStatus.Paid or InvoiceStatus.Voided )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} is {this.Status} and cannot be voided." );
        }

        this.Status = InvoiceStatus.Voided;
        this.VoidReason = reason;
    }

    /// <summary>
    /// Moves the invoice to another department. The user needs <c>ChangeOwnership</c> both on the invoice
    /// and on the destination: otherwise anyone could push an invoice into a department where they have
    /// more rights, or where it does not belong.
    /// </summary>
    [RequiresPermission( PermissionNames.ChangeOwnership )]
    public void TransferTo( [RequiresPermission( PermissionNames.ChangeOwnership )] Department target )
    {
        this.Department = target;
    }

    // The securable lists call this method after they have checked the Write permission, before each change.
    // The permission is already checked, so the method is excluded from default securing.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    void ISecurableCollectionOwner.OnCollectionChanging( string propertyName, AccessKind accessKind )
    {
        if ( propertyName == nameof(this.Lines) )
        {
            this.EnsureStatus( InvoiceStatus.Draft );
        }
    }

    public override string ToString() => $"invoice {this.Number}";

    private void EnsureStatus( InvoiceStatus expected )
    {
        if ( this.Status != expected )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} is {this.Status}, expected {expected}." );
        }
    }
}
