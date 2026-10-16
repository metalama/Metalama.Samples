using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.ExplicitInterface;
public interface IArchivable
{
  void Archive();
  bool IsArchived { get; }
}
public class Document : IArchivable, ISecurable, IInitializable
{
  private bool _isArchived1;
  private bool _isArchived
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document._isArchived", AccessKind.Read);
      return _isArchived1;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document._isArchived", AccessKind.Write);
      _isArchived1 = value;
    }
  }
  // Explicit interface implementations are private, but callers reach them through the interface,
  // so they are secured like public members.
  void IArchivable.Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.IArchivable.Archive", AccessKind.Execute);
    this._isArchived = true;
  }
  bool IArchivable.IsArchived
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.IArchivable.IsArchived", AccessKind.Read);
      return this._isArchived;
    }
  }
  public Document([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Document() : this(context: default)
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