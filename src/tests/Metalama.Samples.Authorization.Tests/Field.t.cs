namespace Metalama.Samples.Authorization.Tests.Field;
public class Document
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
}