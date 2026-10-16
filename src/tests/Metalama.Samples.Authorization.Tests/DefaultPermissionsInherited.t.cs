using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.DefaultPermissionsInherited;
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
// The aspect is [Inheritable], so the derived class gets it without an attribute.
public class Invoice : Entity
{
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