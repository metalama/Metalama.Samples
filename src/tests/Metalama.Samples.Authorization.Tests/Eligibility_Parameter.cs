namespace Metalama.Samples.Authorization.Tests.Eligibility_Parameter;

public class Folder;

public class Document
{
    // An out parameter has no input value to check.
    public bool TryGetFolder( [RequiresPermission( "Read" )] out Folder? folder )
    {
        folder = null;

        return false;
    }

    // A value type cannot be a securable object.
    public void SetPriority( [RequiresPermission( "Write" )] int priority ) { }
}
