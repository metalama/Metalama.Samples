using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.Demo.Finance;
using Microsoft.EntityFrameworkCore;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Application service for budgets.
/// </summary>
public sealed class BudgetService( InvoicingDbContext db )
{
    public Task<Budget> GetAsync( string departmentCode, int fiscalYear, CancellationToken cancellationToken = default )
        => db.Budgets.SingleAsync( b => b.Department.Code == departmentCode && b.FiscalYear == fiscalYear, cancellationToken );

    public async Task<decimal> GetRemainingAsync( string departmentCode, int fiscalYear, CancellationToken cancellationToken = default )
    {
        var budget = await this.GetAsync( departmentCode, fiscalYear, cancellationToken );

        return budget.Remaining;
    }

    public async Task AdjustAsync( string departmentCode, int fiscalYear, decimal delta, CancellationToken cancellationToken = default )
    {
        var budget = await this.GetAsync( departmentCode, fiscalYear, cancellationToken );
        budget.Adjust( delta );
        await db.SaveChangesAsync( cancellationToken );
    }
}
