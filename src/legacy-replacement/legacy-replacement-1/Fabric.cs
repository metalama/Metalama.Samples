using Metalama.Extensions.Interceptors;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        var interceptor = typeof(CallCounter).AsINamedType()
            .Methods.OfName( nameof(CallCounter.ComputeTax) )
            .Single();

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(LegacyTaxCalculator) )
                .Methods( nameof(LegacyTaxCalculator.ComputeTax) )
                .RedirectToExistingMethod( interceptor ) );
    }
}
