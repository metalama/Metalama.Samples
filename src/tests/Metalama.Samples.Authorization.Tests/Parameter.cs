namespace Metalama.Samples.Authorization.Tests.Parameter;

public class Folder;

public class Document
{
    // Only the attributed parameter is checked. The permission is required on the argument, not on this.
    public void MoveTo( [RequiresPermission( "Write" )] Folder target, string? comment ) { }
}
