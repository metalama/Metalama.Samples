namespace Metalama.Samples.Authorization.Tests.Property;

public class Document : ISecurable
{
    // Different permissions for reading and writing.
    [RequiresPermission( "Read", "Approve" )]
    public decimal Amount { get; set; }

    // A single permission applies to both reading and writing.
    [RequiresPermission( "Edit" )]
    public string? Title { get; set; }

    // A get-only property only gets a getter check.
    [RequiresPermission( "Read" )]
    public string Number { get; } = "D-1";

    // A private setter is an implementation detail and is not checked.
    [RequiresPermission( "Read", "Write" )]
    public string? Status { get; private set; }

    // Without arguments, the default permissions of AuthorizationOptions apply.
    [RequiresPermission]
    public DateOnly DueDate { get; set; }
}
