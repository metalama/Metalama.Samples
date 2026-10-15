using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// An invoice issued by a <see cref="Department"/> to a <see cref="Customer"/>.
/// </summary>
/// <remarks>
/// <para>Permission checks come from three sources: the <see cref="ApplyDefaultPermissionsAttribute"/> aspect
/// inherited from <see cref="Entity"/> (properties need <c>Read</c>/<c>Write</c>), the project fabric (methods
/// without an attribute need <c>Write</c>), and explicit <see cref="RequiresPermissionAttribute"/> attributes.</para>
/// <para>Authorization answers "may this user do this?". Business rules, such as "an approved invoice cannot be
/// edited", are a separate concern and are enforced by the methods themselves.</para>
/// </remarks>
public sealed class Invoice : Entity
{
    private readonly List<InvoiceLine> _lines = [];
    private Department _department;
    private InvoiceStatus _status;
    private decimal _amountPaid;

    // Used by Entity Framework, which then sets the backing fields. A constructor must not call a secured setter:
    // Entity Framework runs it when loading the invoice, typically without any permission.
    private Invoice()
    {
        this._department = null!;
        this.Number = null!;
    }

    internal Invoice( Department department, Customer customer, string number )
    {
        this._department = department;
        this.Customer = customer;
        this.Number = number;
        this.DueDate = DateOnly.FromDateTime( DateTime.Today ).AddDays( 30 );
    }

    /// <summary>
    /// Gets the invoice number. It is printed on every document and quoted by customers on the phone,
    /// so it is excluded from default securing.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string Number { get; }

    public Customer Customer { get; set; } = null!;

    public DateOnly DueDate { get; set; }

    public Department Department => this._department;

    public InvoiceStatus Status => this._status;

    public IReadOnlyList<InvoiceLine> Lines => this._lines;

    public decimal Total => this._lines.Sum( l => l.Amount );

    public decimal AmountPaid => this._amountPaid;

    public string? VoidReason { get; private set; }

    protected override IRoleBasedSecurable? SecurityParent => this._department;

    // No attribute: the project fabric makes Write the default permission of methods.
    public void AddLine( string description, decimal quantity, decimal unitPrice )
    {
        this.EnsureStatus( InvoiceStatus.Draft );
        this._lines.Add( new InvoiceLine( description, quantity, unitPrice ) );
    }

    // No attribute: requires Write too.
    public void RemoveLine( int index )
    {
        this.EnsureStatus( InvoiceStatus.Draft );
        this._lines.RemoveAt( index );
    }

    [RequiresPermission( DemoPermissions.Approve )]
    public void Approve()
    {
        this.EnsureStatus( InvoiceStatus.Draft );

        if ( this._lines.Count == 0 )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} has no line." );
        }

        this._status = InvoiceStatus.Approved;
    }

    /// <summary>
    /// Sends the approved invoice to the customer through <paramref name="sender"/>.
    /// </summary>
    [RequiresPermission( DemoPermissions.Send )]
    public async Task SendAsync( IInvoiceSender sender, CancellationToken cancellationToken = default )
    {
        this.EnsureStatus( InvoiceStatus.Approved );

        await sender.SendAsync( this, cancellationToken );

        this._status = InvoiceStatus.Sent;
    }

    [RequiresPermission( DemoPermissions.RecordPayment )]
    public void RecordPayment( decimal amount )
    {
        this.EnsureStatus( InvoiceStatus.Sent );

        this._amountPaid += amount;

        if ( this._amountPaid >= this._lines.Sum( l => l.Amount ) )
        {
            this._status = InvoiceStatus.Paid;
        }
    }

    [RequiresPermission( DemoPermissions.Void )]
    public void Void( string reason )
    {
        if ( this._status is InvoiceStatus.Paid or InvoiceStatus.Voided )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} is {this._status} and cannot be voided." );
        }

        this._status = InvoiceStatus.Voided;
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
        this._department = target;
    }

    public override string ToString() => $"invoice {this.Number}";

    private void EnsureStatus( InvoiceStatus expected )
    {
        if ( this._status != expected )
        {
            throw new InvalidOperationException( $"Invoice {this.Number} is {this._status}, expected {expected}." );
        }
    }
}
