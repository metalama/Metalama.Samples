namespace Metalama.Samples.Authorization.Tests.Options_NonPublic;

[AuthorizationOptions( SecureNonPublicMembers = true )]
public class Document : ISecurable
{
    internal string? InternalNotes { get; set; }

    protected string? ProtectedNotes { get; set; }

    // A field or a property is secured whatever its accessibility, because it holds the state of the object.
    private string? PrivateNotes { get; set; }

    public string? Summary => this.PrivateNotes;

    // The option secures the internal and protected methods. A private method is never secured by default: it runs
    // with the permissions of its caller, and its accesses to the state are checked.
    internal void Archive() { }

    protected void Restore() { }

    private void Clear() => this.PrivateNotes = null;
}
