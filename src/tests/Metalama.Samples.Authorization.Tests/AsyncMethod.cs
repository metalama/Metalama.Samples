namespace Metalama.Samples.Authorization.Tests.AsyncMethod;

public class Document
{
    // The check runs at the start of the async method, so a denial faults the returned task.
    [RequiresPermission( "Send" )]
    public async Task SendAsync( CancellationToken cancellationToken = default )
    {
        await Task.Delay( 1, cancellationToken );
    }
}
