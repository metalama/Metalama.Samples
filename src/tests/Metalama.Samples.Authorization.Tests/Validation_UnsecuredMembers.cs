namespace Metalama.Samples.Authorization.Tests.Validation_UnsecuredMembers;

[ApplyDefaultPermissions]
public class Document
{
    private readonly string[] _pages = ["Cover"];

    // Secured by default: no warning.
    public string? Title { get; set; }

    // A static member cannot be secured: warning AUTH002.
    public static Document Create() => new();

    // The aspect does not support indexers and events: warning AUTH001.
    public string this[ int index ] => this._pages[index];

    public event EventHandler? Changed;

    // Excluded explicitly: no warning.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public static int MaxPages => 100;

    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public event EventHandler? Viewed;
}
