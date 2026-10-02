using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender
            .SelectTypes()
            .Where( type => type.Accessibility == Accessibility.Public )
            .SelectMany( type => type.Methods )
            .Where( method =>
                        method.Accessibility == Accessibility.Public && method.Name != "ToString" )
            .AddAspectIfEligible<ReportAndSwallowExceptionsAttribute>();
}