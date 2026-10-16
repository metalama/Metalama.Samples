namespace Metalama.Samples.Authorization.Tests.GrantsPermissions;

public class Folder : ISecurable
{
    [RequiresPermission( "ReadSensitive" )]
    public string? Owner { get; set; }
}

public class Report : ISecurable
{
    [RequiresPermission( "Read", "Write" )]
    public string? Body { get; set; }

    // The check of the member runs first, outside of the grant, so the grant cannot satisfy it.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( "Read" )]
    public int BodyLength => this.Body?.Length ?? 0;

    // The check of the parameter also runs before the grant.
    [RequiresPermission( "Publish" )]
    [GrantsPermissions( "Read", "Write" )]
    public void Publish( [RequiresPermission( "Read" )] Folder folder ) => this.Body += " (published)";

    // On a parameter, the grant is on the argument, after the check of the argument.
    [RequiresPermission( "Publish" )]
    public bool IsOwnedBySomeone( [RequiresPermission( "Read" )] [GrantsPermissions( "ReadSensitive" )] Folder folder ) => folder.Owner != null;

    // The aspect is inherited: the overrides grant the same permissions.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( "Read" )]
    public virtual int Score => this.Body?.Length ?? 0;

    // In an async method, the grant covers the whole execution, including the continuations.
    [RequiresPermission( "Publish" )]
    [GrantsPermissions( "Read" )]
    public async Task<int> CountWordsAsync( CancellationToken cancellationToken )
    {
        await Task.Delay( 1, cancellationToken );

        return this.Body?.Split( ' ' ).Length ?? 0;
    }
}

public class WeightedReport : Report
{
    public override int Score => 2 * (this.Body?.Length ?? 0);
}

// The check can also come from the default securing of SecurableType.
public class Summary : ISecurable
{
    public string? Text { get; set; }

    [GrantsPermissions( "Read" )]
    public int TextLength => this.Text?.Length ?? 0;
}
