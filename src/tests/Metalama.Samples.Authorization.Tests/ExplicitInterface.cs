namespace Metalama.Samples.Authorization.Tests.ExplicitInterface;

public interface IArchivable
{
    void Archive();

    bool IsArchived { get; }
}

public class Document : IArchivable, ISecurable
{
    private bool _isArchived;

    // Explicit interface implementations are private, but callers reach them through the interface,
    // so they are secured like public members.
    void IArchivable.Archive() => this._isArchived = true;

    bool IArchivable.IsArchived => this._isArchived;
}
