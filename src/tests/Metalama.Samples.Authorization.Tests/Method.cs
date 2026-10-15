namespace Metalama.Samples.Authorization.Tests.Method;

public class Document
{
    [RequiresPermission( "Approve" )]
    public void Approve() { }

    [RequiresPermission( "Read" )]
    public string Render( string format ) => $"Document as {format}";
}
