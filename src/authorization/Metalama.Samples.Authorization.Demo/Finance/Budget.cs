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
