namespace Metalama.Samples.Authorization.Tests.NoDefaultExecutePermission;

public class Document
{
    // There is no default permission for methods, so the attribute must name one.
    [RequiresPermission]
    public void Archive() { }
}
