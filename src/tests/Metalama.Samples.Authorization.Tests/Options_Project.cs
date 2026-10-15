using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization.Tests.Options_Project;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.SetOptions( new AuthorizationOptions { DefaultReadPermission = "View", DefaultWritePermission = "Edit" } );
}

public class Document : ISecurable
{
    public string? Title { get; set; }
}
