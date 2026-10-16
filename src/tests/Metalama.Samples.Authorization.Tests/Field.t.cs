using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Field;
public class Document : ISecurable, IInitializable
{
  // Metalama turns the field into a property to intercept its accesses.
  private string? _notes;
  [RequiresPermission("Read", "Write")]
  public string? Notes
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Notes", AccessKind.Read);
      return _notes;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.Notes", AccessKind.Write);
      _notes = value;
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