using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization.Demo.Finance;

/// <summary>
/// Finance data is confidential: in this namespace, reading a field or property requires
/// <see cref="DemoPermissions.ReadSensitive"/> instead of <see cref="PermissionNames.Read"/>.
/// </summary>
internal sealed class FinanceFabric : NamespaceFabric
{
    public override void AmendNamespace( INamespaceAmender amender )
        => amender.SetOptions( new AuthorizationOptions { DefaultReadPermission = DemoPermissions.ReadSensitive } );
}
