using Metalama.Framework.RunTime.Initialization;
using Metalama.Samples.Authorization.RoleBased;
namespace Metalama.Samples.Authorization.Demo.Finance;
public sealed class Forecast : Entity
{
  private decimal _amount;
  // Reading requires ReadSensitive, from the namespace fabric. Writing keeps the default.
  public decimal Amount
  {
    get
    {
      AccessGuard.Demand(this, "ReadSensitive", "Forecast.Amount", AccessKind.Read);
      return _amount;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Forecast.Amount", AccessKind.Write);
      _amount = value;
    }
  }
  // Requires Write, the default permission of methods.
  public void Revise(decimal amount)
  {
    AccessGuard.Demand(this, "Write", "Forecast.Revise", AccessKind.Execute);
    this.Amount = amount;
  }
  protected override IRoleBasedSecurable? SecurityParent => null;
  public Forecast(InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    this.OnConstructed(context);
  }
  public Forecast() : this(context: default)
  {
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
  protected override void OnConstructed(InitializationContext context = default)
  {
    base.OnConstructed(context);
    SecurityContext.EndConstruction(this);
  }
}