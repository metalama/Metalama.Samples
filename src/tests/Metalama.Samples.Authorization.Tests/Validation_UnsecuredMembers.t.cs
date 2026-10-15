// Warning AUTH002 on `Create`: `'Document.Create()' is a static member of the secured type 'Document'. A static member has no instance on which to check a permission. Move it to another type, or exclude it explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )].`
// Warning AUTH001 on `this`: `'Document.this[int]' is exposed by the secured type 'Document' but has no permission check. Add [RequiresPermission], or exclude the member explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )].`
// Warning AUTH001 on `Changed`: `'Document.Changed' is exposed by the secured type 'Document' but has no permission check. Add [RequiresPermission], or exclude the member explicitly with [AuthorizationOptions( IsDefaultSecuringEnabled = false )].`
namespace Metalama.Samples.Authorization.Tests.Validation_UnsecuredMembers;
[SecurableType]
public class Document
{
  private readonly string[] _pages = ["Cover"];
  private string? _title;
  // Secured by default: no warning.
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
  // A static member cannot be secured: warning AUTH002.
  public static Document Create() => new();
  // The aspect does not support indexers and events: warning AUTH001.
  public string this[int index] => this._pages[index];
  public event EventHandler? Changed;
  // Excluded explicitly: no warning.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public static int MaxPages => 100;
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public event EventHandler? Viewed;
}