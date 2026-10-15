namespace Metalama.Samples.Authorization.Tests.Method;
public class Document
{
  [RequiresPermission("Approve")]
  public void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
  [RequiresPermission("Read")]
  public string Render(string format)
  {
    AccessGuard.Demand(this, "Read", "Document.Render", AccessKind.Execute);
    return $"Document as {format}";
  }
}