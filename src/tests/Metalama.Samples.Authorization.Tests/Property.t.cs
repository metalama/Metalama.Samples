namespace Metalama.Samples.Authorization.Tests.Property;
public class Document
{
  private decimal _amount;
  // Different permissions for reading and writing.
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
  private string? _title;
  // A single permission applies to both reading and writing.
  [RequiresPermission("Edit")]
  public string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Edit", "Document.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Edit", "Document.Title", AccessKind.Write);
      _title = value;
    }
  }
  private readonly string _number = "D-1";
  // A get-only property only gets a getter check.
  [RequiresPermission("Read")]
  public string Number
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Number", AccessKind.Read);
      return _number;
    }
    private init
    {
      _number = value;
    }
  }
  private string? _status;
  // A private setter is an implementation detail and is not checked.
  [RequiresPermission("Read", "Write")]
  public string? Status
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Status", AccessKind.Read);
      return _status;
    }
    private set
    {
      _status = value;
    }
  }
  private DateOnly _dueDate;
  // Without arguments, the default permissions of AuthorizationOptions apply.
  [RequiresPermission]
  public DateOnly DueDate
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.DueDate", AccessKind.Read);
      return _dueDate;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.DueDate", AccessKind.Write);
      _dueDate = value;
    }
  }
}