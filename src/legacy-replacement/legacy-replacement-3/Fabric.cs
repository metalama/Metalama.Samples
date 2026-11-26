using Metalama.Extensions.Interceptors;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using System.Collections.Immutable;

internal class Fabric : ProjectFabric
{
    // [<snippet Modules>]
    // The migration stage of each module. The calls in the modules that are not listed
    // still go to the legacy calculator.
    private static readonly ImmutableDictionary<string, MigrationStage> _modules =
        ImmutableDictionary<string, MigrationStage>.Empty
            .Add( "Billing", MigrationStage.Migrated )
            .Add( "Shipping", MigrationStage.Comparing );
    // [<endsnippet Modules>]

    // [<snippet Registration>]
    public override void AmendProject( IProjectAmender amender )
        => amender
            .SelectMany(
                compilation => _modules.Keys
                    .Select( name => compilation.GlobalNamespace.GetDescendant( name ) )
                    .OfType<INamespace>() )
            .WithTag( ns => _modules[ns.FullName] )
            .InterceptMethods(
                calls => calls
                    .Type( typeof(LegacyTaxCalculator) )
                    .Methods( nameof(LegacyTaxCalculator.ComputeTax) )
                    .ForEachSite( site => site.RedirectToExistingMethod( GetInterceptor( site.ScopeTag ) ) ) );
    // [<endsnippet Registration>]

    private static IMethod GetInterceptor( object? stage )
    {
        var interceptorType = stage switch
        {
            MigrationStage.Comparing => typeof(TaxComparison),
            MigrationStage.Migrated => typeof(NewTaxCalculatorAdapter),
            _ => throw new ArgumentOutOfRangeException( nameof(stage) )
        };

        return interceptorType.AsINamedType().Methods.OfName( "ComputeTax" ).Single();
    }

    private enum MigrationStage
    {
        // The calls go to the legacy calculator, and some of them are compared with the new calculator.
        Comparing,

        // The calls go to the new calculator.
        Migrated
    }
}
