namespace Metalama.Samples.Authorization.Tests.Options_DefaultExecute;
[ApplyDefaultPermissions]
[AuthorizationOptions(DefaultExecutePermission = "Write")]
public class Document
{
  private readonly List<string> _lines = [];
  // Methods are secured by default.
  public void AddLine(string line)
  {
    AccessGuard.Demand(this, "Write", "Document.AddLine", AccessKind.Execute);
    this._lines.Add(line);
  }
  // An explicit permission wins.
  [RequiresPermission("Approve")]
  public void Approve()
  {
    AccessGuard.Demand(this, "Approve", "Document.Approve", AccessKind.Execute);
  }
  // Overrides of object methods are never secured by default.
  public override string ToString() => string.Join(", ", this._lines);
}