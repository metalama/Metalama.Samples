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

    // Non-public and static members are not secured by default.
    internal string? InternalNotes { get; set; }

    public static int Count { get; set; }

    // Methods are not secured unless AuthorizationOptions.DefaultExecutePermission is set.
    public void Publish() => this.Title = this._draft;

    public override string ToString() => this.Title ?? "";
}
