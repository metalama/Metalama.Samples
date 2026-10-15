using Metalama.Extensions.Interceptors;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

internal class Fabric : ProjectFabric
{
    // [<snippet Modules>]
    // The migration stage of each module. The calls in the modules that are not listed
    // still go to the legacy calculator.
    private static readonly Dictionary<string, MigrationStage> _modules = new()
    {
        ["Billing"] = MigrationStage.Migrated,
        ["Shipping"] = MigrationStage.Comparing
    };
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
