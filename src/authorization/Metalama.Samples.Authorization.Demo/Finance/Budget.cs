using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo.Finance;

/// <summary>
/// The yearly budget of a <see cref="Department"/>. Because of <see cref="FinanceFabric"/>, reading any member
/// requires <see cref="DemoPermissions.ReadSensitive"/>, although no member of this class has an attribute.
/// </summary>
public sealed class Budget( Department department, int fiscalYear, decimal amount ) : Entity
{
    private decimal _amount = amount;
    private decimal _spent;

    // Used by Entity Framework, which creates entities in a system scope.
    private Budget() : this( null!, 0, 0 ) { }

    public Department Department { get; } = department;

    public int FiscalYear { get; } = fiscalYear;

    public decimal Amount => this._amount;

    public decimal Spent => this._spent;

    public decimal Remaining => this._amount - this._spent;

    /// <summary>
    /// Gets the level of the budget, without its amounts. It requires only <see cref="PermissionNames.Read"/>, so the
    /// staff of the department can see it. It is computed from <see cref="Amount"/> and <see cref="Remaining"/>, which
    /// require <see cref="DemoPermissions.ReadSensitive"/>, so the property grants this permission while it runs.
    /// </summary>
    [RequiresPermission( PermissionNames.Read )]
    [GrantsPermissions( DemoPermissions.ReadSensitive )]
    public BudgetStatus Status
        => this.Remaining <= 0 ? BudgetStatus.Exhausted
            : this.Remaining < this.Amount / 10 ? BudgetStatus.Low
            : BudgetStatus.Available;

    protected override IRoleBasedSecurable? SecurityParent => this.Department;

    // No attribute: Write is the default permission of methods.
    public void Adjust( decimal delta ) => this._amount += delta;

    public void RecordSpending( decimal amount )
    {
        if ( amount > this.Remaining )
        {
            throw new InvalidOperationException( $"Spending {amount} exceeds the remaining budget of {this.Department.Code}." );
        }

        this._spent += amount;
    }

    public override string ToString() => $"budget {this.Department.Code} {this.FiscalYear}";
}
