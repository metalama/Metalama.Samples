// Warning AUTH002 on `Count`: `'Document.Count' is a static member of the secured type 'Document'. A static member has no instance on which to check a permission. Move it to another type, or exclude it explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )].`
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.DefaultPermissions;
public class Document : ISecurable, IInitializable
{
  private string? _draft1 = "Draft";
  private string? _draft
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document._draft", AccessKind.Read);
      return _draft1;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document._draft", AccessKind.Write);
      _draft1 = value;
    }
  }
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
  private string? _internalNotes;
  // Fields and properties are secured whatever their accessibility, because they hold the state of the object.
  internal string? InternalNotes
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.InternalNotes", AccessKind.Read);
      return _internalNotes;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.InternalNotes", AccessKind.Write);
      _internalNotes = value;
    }
  }
  // Non-public methods are not secured by default.
  internal void Reset() => this._draft = null;
  // A static member cannot be secured: the validator reports warning AUTH002.
  public static int Count { get; set; }
  // Methods are secured too, with Write by default.
  public void Publish()
  {
    AccessGuard.Demand(this, "Write", "Document.Publish", AccessKind.Execute);
    this.Title = this._draft;
  }
  public override string ToString() => this.Title ?? "";
  public Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}