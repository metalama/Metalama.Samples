namespace Metalama.Samples.Authorization.Tests.ExplicitInterface;
public interface IArchivable
{
  void Archive();
  bool IsArchived { get; }
}
[ApplyDefaultPermissions]
public class Document : IArchivable
{
  private bool _isArchived;
  // Explicit interface implementations are private, but callers reach them through the interface,
  // so they are secured like public members.
  void IArchivable.Archive()
  {
    AccessGuard.Demand(this, "Write", "Document.IArchivable.Archive", AccessKind.Execute);
    this._isArchived = true;
  }
  bool IArchivable.IsArchived
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.IArchivable.IsArchived", AccessKind.Read);
      return this._isArchived;
    }
  }
}