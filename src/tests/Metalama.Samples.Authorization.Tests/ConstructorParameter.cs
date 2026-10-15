namespace Metalama.Samples.Authorization.Tests.ConstructorParameter;

public class Folder;

public class Document
{
    // Creating a document in a folder requires a permission on the folder.
    public Document( [RequiresPermission( "AddDocument" )] Folder folder )
    {
        this.Folder = folder;
    }

    public Folder Folder { get; }
}
