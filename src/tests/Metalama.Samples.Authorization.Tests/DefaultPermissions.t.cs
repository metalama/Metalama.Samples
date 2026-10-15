namespace Metalama.Samples.Authorization.Tests.DefaultPermissions;
[ApplyDefaultPermissions]
public class Document
{
  private string? _draft = "Draft";
  private string? _title;
  // Public instance members get the default Read and Write permissions.
  public string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.Title", AccessKind.Write);
      _title = value;
    }
  }
  private string? _notes;
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
  private decimal _amount;
  // An explicit attribute wins over the defaults.
  [RequiresPermission("Read", "Approve")]
  public decimal Amount
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Amount", AccessKind.Read);
      return _amount;
    }
    set
    {
      AccessGuard.Demand(this, "Approve", "Document.Amount", AccessKind.Write);
      _amount = value;
    }
  }
  // Non-public and static members are not secured by default.
  internal string? InternalNotes { get; set; }
  public static int Count { get; set; }
  // Methods are not secured unless AuthorizationOptions.DefaultExecutePermission is set.
  public void Publish() => this.Title = this._draft;
  public override string ToString() => this.Title ?? "";
}