namespace Metalama.Samples.Authorization.Tests.Field;

public class Document
{
    // Metalama turns the field into a property to intercept its accesses.
    [RequiresPermission( "Read", "Write" )]
    public string? Notes;
}
