using Metalama.Framework.RunTime.Initialization;
using Metalama.Samples.Authorization.RoleBased;
namespace Metalama.Samples.Authorization.Demo.Sales;
public sealed class Quote : Entity
{
  private decimal _amount;
  // Read and Write: the library defaults.
  public decimal Amount
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Quote.Amount", AccessKind.Read);
      return _amount;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Quote.Amount", AccessKind.Write);
      _amount = value;
    }
  }
  private string? _internalNote;
  // Secured because the project fabric sets SecureNonPublicMembers.
  internal string? InternalNote
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Quote.InternalNote", AccessKind.Read);
      return _internalNote;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Quote.InternalNote", AccessKind.Write);
      _internalNote = value;
    }
  }
  // Requires Write, the default permission of methods.
  public void AddItem(string description)
  {
    AccessGuard.Demand(this, "Write", "Quote.AddItem", AccessKind.Execute);
  }
  [RequiresPermission(DemoPermissions.Approve)]
  public void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Quote.Approve", AccessKind.Execute);
  }
  // Not secured: the option set on the base member in the library is inherited by the override.
  protected override IRoleBasedSecurable? SecurityParent => null;
  public override string ToString() => "quote";
  public Quote()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}