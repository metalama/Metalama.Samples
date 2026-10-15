namespace Metalama.Samples.Authorization.Tests.Inheritance_DefaultOverride;
[ApplyDefaultPermissions]
[AuthorizationOptions(DefaultExecutePermission = "Write")]
public class Document
{
  // Secured by default.
  public virtual void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
  }
  // Secured explicitly with another permission.
  [RequiresPermission("Approve")]
  public virtual void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
}
// Each override gets exactly one check, with the permission of the base member.
public class Contract : Document
{
  public override void Archive()
  {
    AccessGuard.Demand(this, "Write", "Contract.Archive", AccessKind.Execute);
  }
  public override void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Contract.Approve", AccessKind.Execute);
  }
}