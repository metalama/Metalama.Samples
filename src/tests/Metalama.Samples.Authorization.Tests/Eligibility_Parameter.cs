namespace Metalama.Samples.Authorization.Tests.Eligibility_Parameter;

public class Folder : ISecurable;

public class Document
{
    // An out parameter has no input value to check.
    public bool TryGetFolder( [RequiresPermission( "Read" )] out Folder? folder )
    {
        folder = null;

        return false;
    }

    // Only a securable object can be checked: a value type, or a reference type that is not securable, is an error.
    public void SetPriority( [RequiresPermission( "Write" )] int priority ) { }

    public void Rename( [RequiresPermission( "Write" )] string name ) { }
}
