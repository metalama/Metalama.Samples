namespace Metalama.Samples.Authorization.Tests.Options_DefaultExecute;
[AuthorizationOptions(DefaultExecutePermission = "Edit")]
public class Document : ISecurable
{
  private readonly IList<string> _lines1 = new SecurableList<string>();
  private IList<string> _lines
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document._lines", AccessKind.Read);
      return SecurableCollection.SetOwner(_lines1, this, "Write", "Document._lines", "_lines");
    }
    init
    {
      _lines1 = value;
    }
  }
  // Methods require the configured default permission instead of Write.
  public void AddLine(string line)
  {
    AccessGuard.Demand(this, "Edit", "Document.AddLine", AccessKind.Execute);
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