using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_NonPublic;
[AuthorizationOptions(SecureNonPublicMembers = true)]
public class Document : ISecurable, IInitializable
{
  private string? _internalNotes;
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
  private string? _protectedNotes;
  protected string? ProtectedNotes
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.ProtectedNotes", AccessKind.Read);
      return _protectedNotes;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.ProtectedNotes", AccessKind.Write);
      _protectedNotes = value;
    }
  }
  private string? _privateNotes;
  // A field or a property is secured whatever its accessibility, because it holds the state of the object.
  private string? PrivateNotes
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.PrivateNotes", AccessKind.Read);
      return _privateNotes;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.PrivateNotes", AccessKind.Write);
      _privateNotes = value;
    }
  }
  public string? Summary
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Summary", AccessKind.Read);
      return this.PrivateNotes;
    }
  }
  // The option secures the internal and protected methods. A private method is never secured by default: it runs
  // with the permissions of its caller, and its accesses to the state are checked.
  internal void Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.Archive", AccessKind.Execute);
  }
  protected void Restore()
  {
    AccessGuard.Demand(this, "Write", "Document.Restore", AccessKind.Execute);
  }
  private void Clear() => this.PrivateNotes = null;
  public Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}