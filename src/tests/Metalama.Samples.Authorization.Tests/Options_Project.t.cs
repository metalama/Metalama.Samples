using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_Project;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class Fabric : ProjectFabric
{
  public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class Document : ISecurable, IInitializable
{
  private string? _title;
  public string? Title
  {
    get
    {
      AccessGuard.Demand(this, "View", "Document.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Edit", "Document.Title", AccessKind.Write);
      _title = value;
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