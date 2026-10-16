using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Inheritance_Override;
public class Document : ISecurable, IInitializable
{
  [RequiresPermission("Approve")]
  public virtual void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
  private string? _title;
  [RequiresPermission("Read", "Write")]
  public virtual string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.Title", AccessKind.Write);
      _title = value;
    }
  }
  public Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
// The aspect is inherited: the overrides are checked although they have no attribute
// and do not call the base implementation.
public class Contract : Document
{
  public override void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Contract.Approve", AccessKind.Execute);
  }
  private string? _title;
  public override string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Contract.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Contract.Title", AccessKind.Write);
      _title = value;
    }
  }
  public Contract()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}