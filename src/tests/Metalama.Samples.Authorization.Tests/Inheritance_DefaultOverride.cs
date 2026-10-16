namespace Metalama.Samples.Authorization.Tests.Inheritance_DefaultOverride;

[AuthorizationOptions( DefaultExecutePermission = "Write" )]
public class Document : ISecurable
{
    // Secured by default.
    public virtual void Archive() { }

    // Secured explicitly with another permission.
    [RequiresPermission( "Approve" )]
    public virtual void Approve() { }
}

// Each override gets exactly one check, with the permission of the base member.
public class Contract : Document
{
    public override void Archive() { }

    public override void Approve() { }
}
