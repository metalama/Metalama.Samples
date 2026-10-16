namespace Metalama.Samples.Authorization.Tests.DefaultPermissions;

public class Document : ISecurable
{
    private string? _draft = "Draft";

    // Public instance members get the default Read and Write permissions.
    public string? Title { get; set; }

    public string? Notes;

    // An explicit attribute wins over the defaults.
    [RequiresPermission( "Read", "Approve" )]
    public decimal Amount { get; set; }

    // Fields and properties are secured whatever their accessibility, because they hold the state of the object.
    internal string? InternalNotes { get; set; }

    // Non-public methods are not secured by default.
    internal void Reset() => this._draft = null;

    // A static member cannot be secured: the validator reports warning AUTH002.
    public static int Count { get; set; }

    // Methods are secured too, with Write by default.
    public void Publish() => this.Title = this._draft;

    public override string ToString() => this.Title ?? "";
}
