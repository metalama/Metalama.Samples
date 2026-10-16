using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_Disabled;
public abstract class Entity : ISecurable, IInitializable
{
  private string? _name;
  public string? Name
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Entity.Name", AccessKind.Read);
      return _name;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Entity.Name", AccessKind.Write);
      _name = value;
    }
  }
  protected Entity()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
// Default securing can be disabled for a type...
[AuthorizationOptions(IsDefaultSecuringEnabled = false)]
public class LookupValue : Entity
{
  public string? Code { get; set; }
  public LookupValue()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}
public class Invoice : Entity
{
  // ...or for a single member.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public string? Number { get; set; }
  private decimal _amount;
  public decimal Amount
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Invoice.Amount", AccessKind.Read);
      return _amount;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Invoice.Amount", AccessKind.Write);
      _amount = value;
    }
  }
  public Invoice()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}