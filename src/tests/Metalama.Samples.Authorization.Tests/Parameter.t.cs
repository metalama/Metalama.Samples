namespace Metalama.Samples.Authorization.Tests.Parameter;
public class Folder : ISecurable;
public class Document
{
  // Only the attributed parameter is checked. The permission is required on the argument, not on this.
  public void MoveTo([RequiresPermission("Write")] Folder target, string? comment)
  {
    object? argument = target;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Write", "Document.MoveTo(target)", AccessKind.Execute);
    }
  }
}