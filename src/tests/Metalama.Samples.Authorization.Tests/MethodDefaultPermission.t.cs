namespace Metalama.Samples.Authorization.Tests.MethodDefaultPermission;
public class Document
{
  // Without arguments, a method requires the default execute permission, which is Write.
  [RequiresPermission]
  public void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
  }
}
[SecurableType]
public class Report
{
  public void Refresh()
  {
    AccessGuard.Demand(this, "Write", "Report.Refresh", AccessKind.Execute);
  }
  // A method is excluded from default securing only explicitly.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public string Preview() => "Preview";
}