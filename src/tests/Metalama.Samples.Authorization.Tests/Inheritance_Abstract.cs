namespace Metalama.Samples.Authorization.Tests.Inheritance_Abstract;

public abstract class Document
{
    // An abstract member has no body to check, but the aspect is inherited by all overrides.
    [RequiresPermission( "Approve" )]
    public abstract void Approve();
}

public class Contract : Document
{
    public override void Approve() { }
}
