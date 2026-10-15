namespace Metalama.Samples.Authorization.Tests.Options_NonPublic;
[SecurableType]
[AuthorizationOptions(SecureNonPublicMembers = true)]
public class Document
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
  // Private members are never secured by default.
  private string? PrivateNotes { get; set; }
  public string? Summary
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Summary", AccessKind.Read);
      return this.PrivateNotes;
    }
  }
}