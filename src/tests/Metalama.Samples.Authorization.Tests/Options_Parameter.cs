namespace Metalama.Samples.Authorization.Tests.Options_Parameter;

public class Folder;

public class Document
{
    // Without arguments, a parameter check requires the default read permission.
    public void CopyTo( [RequiresPermission] Folder target ) { }

    // Options set on the method apply to its parameters.
    [AuthorizationOptions( DefaultReadPermission = "Write" )]
    public void MoveTo( [RequiresPermission] Folder target ) { }
}
