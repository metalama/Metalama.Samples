using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization.Tests.Options_Namespace
{
    public class Fabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
            => amender.SetOptions( new AuthorizationOptions { DefaultWritePermission = "Edit" } );
    }

    public abstract class Entity : ISecurable
    {
        public string? Name { get; set; }
    }

    namespace Finance
    {
        // The namespace fabric overrides the project for this namespace only. It also applies to the members
        // of a class whose base class is declared in another namespace.
        public class FinanceFabric : NamespaceFabric
        {
            public override void AmendNamespace( INamespaceAmender amender )
                => amender.SetOptions( new AuthorizationOptions { DefaultReadPermission = "ReadSensitive" } );
        }

        public class Budget : Entity
        {
            public decimal Amount { get; set; }
        }
    }
}
