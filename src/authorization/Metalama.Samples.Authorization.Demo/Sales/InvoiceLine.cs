namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// A line of an <see cref="Invoice"/>. Lines are values owned by the invoice, so they are not secured separately.
/// </summary>
public sealed record InvoiceLine( string Description, decimal Quantity, decimal UnitPrice )
{
    public decimal Amount => this.Quantity * this.UnitPrice;
}
