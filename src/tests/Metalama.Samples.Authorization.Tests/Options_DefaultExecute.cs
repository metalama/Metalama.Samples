namespace Metalama.Samples.Authorization.Tests.Options_DefaultExecute;

[AuthorizationOptions( DefaultExecutePermission = "Edit" )]
public class Document : ISecurable
{
    private readonly IList<string> _lines = new SecurableList<string>();

    // Methods require the configured default permission instead of Write.
    public void AddLine( string line ) => this._lines.Add( line );

    // An explicit permission wins.
    [RequiresPermission( "Approve" )]
    public void Approve() { }

    // Overrides of object methods are never secured by default.
    public override string ToString() => string.Join( ", ", this._lines );
}
