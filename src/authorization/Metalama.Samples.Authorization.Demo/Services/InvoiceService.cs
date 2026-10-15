using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Application service for invoices. It loads the objects, calls the domain methods, and saves the changes.
/// It contains no permission check: the domain objects enforce them.
/// </summary>
public sealed class InvoiceService( InvoicingDbContext db, IInvoiceSender sender )
{
    public Task<Invoice> GetAsync( string number, CancellationToken cancellationToken = default )
        => db.Invoices.SingleAsync( i => i.Number == number, cancellationToken );

    public async Task<Invoice> CreateAsync( string departmentCode, string customerName, CancellationToken cancellationToken = default )
    {
        var department = await db.Departments.SingleAsync( d => d.Code == departmentCode, cancellationToken );
        var customer = await db.Customers.SingleAsync( c => c.Name == customerName, cancellationToken );

        var invoice = department.CreateInvoice( customer );
        db.Invoices.Add( invoice );
        await db.SaveChangesAsync( cancellationToken );

        return invoice;
    }

    public async Task AddLineAsync(
        string number,
        string description,
        decimal quantity,
        decimal unitPrice,
        CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.Lines.Add( new InvoiceLine( description, quantity, unitPrice ) );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task AddNoteAsync( string number, string note, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.Notes.Add( note );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task ChangeCustomerAsync( string number, string customerName, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        var customer = await db.Customers.SingleAsync( c => c.Name == customerName, cancellationToken );
        invoice.ChangeCustomer( customer );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task ChangeOwnerAsync( string number, string userName, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.Owner = await db.Users.SingleAsync( u => u.Name == userName, cancellationToken );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task ApproveAsync( string number, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.Approve();
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task SendAsync( string number, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        await invoice.SendAsync( sender, cancellationToken );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task RecordPaymentAsync( string number, decimal amount, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.RecordPayment( amount );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task VoidAsync( string number, string reason, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        invoice.Void( reason );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task TransferAsync( string number, string departmentCode, CancellationToken cancellationToken = default )
    {
        var invoice = await this.GetAsync( number, cancellationToken );
        var department = await db.Departments.SingleAsync( d => d.Code == departmentCode, cancellationToken );
        invoice.TransferTo( department );
        await db.SaveChangesAsync( cancellationToken );
    }
}
