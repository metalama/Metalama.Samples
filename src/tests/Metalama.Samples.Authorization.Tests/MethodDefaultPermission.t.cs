using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.MethodDefaultPermission;
public class Document : ISecurable, IInitializable
{
  // Without arguments, a method requires the default execute permission, which is Write.
  [RequiresPermission]
  public void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
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
public class Report : ISecurable, IInitializable
{
  public void Refresh()
  {
    AccessGuard.Demand(this, "Write", "Report.Refresh", AccessKind.Execute);
  }
  // A method is excluded from default securing only explicitly.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public string Preview() => "Preview";
  public Report()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}