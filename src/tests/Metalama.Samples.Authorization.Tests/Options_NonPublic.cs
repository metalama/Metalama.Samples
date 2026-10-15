namespace Metalama.Samples.Authorization.Tests.Options_NonPublic;

[SecurableType]
[AuthorizationOptions( SecureNonPublicMembers = true )]
public class Document
{
    internal string? InternalNotes { get; set; }

    protected string? ProtectedNotes { get; set; }

    // Private members are never secured by default.
    private string? PrivateNotes { get; set; }

    public string? Summary => this.PrivateNotes;
}
