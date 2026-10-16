using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Project-wide authorization options. They only affect classes with the <see cref="SecurableTypeAttribute"/>
/// aspect, here every <see cref="Entity"/>.
/// </summary>
internal sealed class AuthorizationFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.SetOptions(
            new AuthorizationOptions
            {
                // internal and protected methods are secured too, because other code of this assembly
                // must not bypass checks by calling them. Fields and properties are always secured.
                SecureNonPublicMembers = true
            } );
}
