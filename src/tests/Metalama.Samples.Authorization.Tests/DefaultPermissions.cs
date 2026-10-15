namespace Metalama.Samples.Authorization.Tests.DefaultPermissions;

[ApplyDefaultPermissions]
public class Document
{
    private string? _draft = "Draft";

    // Public instance members get the default Read and Write permissions.
    public string? Title { get; set; }

    public string? Notes;

    // An explicit attribute wins over the defaults.
    [RequiresPermission( "Read", "Approve" )]
    public decimal Amount { get; set; }

    // Non-public members are not secured by default.
    internal string? InternalNotes { get; set; }

    // A static member cannot be secured: the validator reports warning AUTH002.
    public static int Count { get; set; }

    // Methods are secured too, with Write by default.
    public void Publish() => this.Title = this._draft;

    public override string ToString() => this.Title ?? "";
}
