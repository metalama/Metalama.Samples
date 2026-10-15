namespace Metalama.Samples.Authorization.Tests.MethodDefaultPermission;

public class Document
{
    // Without arguments, a method requires the default execute permission, which is Write.
    [RequiresPermission]
    public void Archive() { }
}

[SecurableType]
public class Report
{
    public void Refresh() { }

    // A method is excluded from default securing only explicitly.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string Preview() => "Preview";
}
