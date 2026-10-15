namespace Metalama.Samples.Authorization.Tests.Options_Parameter;
public class Folder : ISecurable;
public class Document
{
  // Without arguments, a parameter check requires the default read permission.
  public void CopyTo([RequiresPermission] Folder target)
  {
    object? argument = target;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Read", "Document.CopyTo(target)", AccessKind.Execute);
    }
  }
  // Options set on the method apply to its parameters.
  [AuthorizationOptions(DefaultReadPermission = "Write")]
  public void MoveTo([RequiresPermission] Folder target)
  {
    object? argument = target;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Write", "Document.MoveTo(target)", AccessKind.Execute);
    }
  }
}