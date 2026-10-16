using Metalama.Framework.RunTime;
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
  protected Entity([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  protected Entity() : this(context: default)
  {
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
  protected virtual void OnConstructed(InitializationContext context = default)
  {
    SecurityContext.EndConstruction(this);
  }
}
// Default securing can be disabled for a type...
[AuthorizationOptions(IsDefaultSecuringEnabled = false)]
public class LookupValue : Entity
{
  public string? Code { get; set; }
  public LookupValue([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public LookupValue() : this(context: default)
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
  public Invoice([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Invoice() : this(context: default)
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