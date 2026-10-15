using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Sales;
using Microsoft.EntityFrameworkCore;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Application service for customers.
/// </summary>
public sealed class CustomerService( InvoicingDbContext db )
{
    public Task<Customer> GetAsync( string name, CancellationToken cancellationToken = default )
        => db.Customers.SingleAsync( c => c.Name == name, cancellationToken );

    public async Task<string?> GetTaxIdAsync( string name, CancellationToken cancellationToken = default )
    {
        var customer = await this.GetAsync( name, cancellationToken );

        return customer.TaxId;
    }

    public async Task ChangeEmailAsync( string name, string email, CancellationToken cancellationToken = default )
    {
        var customer = await this.GetAsync( name, cancellationToken );
        customer.Email = email;
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task ChangeCreditLimitAsync( string name, decimal creditLimit, CancellationToken cancellationToken = default )
    {
        var customer = await this.GetAsync( name, cancellationToken );
        customer.CreditLimit = creditLimit;
        await db.SaveChangesAsync( cancellationToken );
    }
}
