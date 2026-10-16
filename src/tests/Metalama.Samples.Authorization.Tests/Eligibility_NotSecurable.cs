namespace Metalama.Samples.Authorization.Tests.Eligibility_NotSecurable;

// A permission is checked on the current instance, so the declaring type must be securable.
public class Document
{
    [RequiresPermission( "Read" )]
    public string? Title { get; set; }

    [RequiresPermission( "Approve" )]
    public void Approve() { }
}

// SecurableType is inherited from ISecurable. Applying it to another type is an error.
[SecurableType]
public class Note
{
    public string? Text { get; set; }
}
