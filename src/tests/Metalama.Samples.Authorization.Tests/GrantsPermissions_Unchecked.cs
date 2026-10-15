namespace Metalama.Samples.Authorization.Tests.GrantsPermissions_Unchecked;

public class Report : ISecurable
{
    public string? Body { get; set; }

    // Excluded from default securing: without a check, any caller would get the grant.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    [GrantsPermissions( "Read" )]
    public int BodyLength => this.Body?.Length ?? 0;

    // A parameter without a check.
    [RequiresPermission( "Read" )]
    public bool IsSameLength( [GrantsPermissions( "Read" )] Report other ) => other.BodyLength == this.BodyLength;

    // A private member is not secured by default either.
    [GrantsPermissions( "Read" )]
    private int CountWords() => this.Body?.Split( ' ' ).Length ?? 0;
}
