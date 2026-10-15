namespace Metalama.Samples.Authorization.Tests.Inheritance_Override;

public class Document
{
    [RequiresPermission( "Approve" )]
    public virtual void Approve() { }

    [RequiresPermission( "Read", "Write" )]
    public virtual string? Title { get; set; }
}

// The aspect is inherited: the overrides are checked although they have no attribute
// and do not call the base implementation.
public class Contract : Document
{
    public override void Approve() { }

    public override string? Title { get; set; }
}
