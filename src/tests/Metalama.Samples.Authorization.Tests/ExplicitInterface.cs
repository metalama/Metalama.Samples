namespace Metalama.Samples.Authorization.Tests.ExplicitInterface;

public interface IArchivable
{
    void Archive();

    bool IsArchived { get; }
}

[SecurableType]
public class Document : IArchivable
{
    private bool _isArchived;

    // Explicit interface implementations are private, but callers reach them through the interface,
    // so they are secured like public members.
    void IArchivable.Archive() => this._isArchived = true;

    bool IArchivable.IsArchived => this._isArchived;
}
