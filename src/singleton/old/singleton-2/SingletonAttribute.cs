using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

public class SingletonAttribute : TypeAspect
{
    private static readonly DiagnosticDefinition<(IConstructor, INamedType)> _constructorHasToBePrivate
        = new( "SING01", Severity.Warning, "The constructor {0} of the singleton class {1} has to be private." );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach ( var constructor in builder.Target.Constructors )
        {
            if ( constructor.Accessibility != Accessibility.Private )
            {
                builder.Diagnostics.Report( _constructorHasToBePrivate.WithArguments( (constructor, builder.Target) ), location: constructor );
            }
        }
    }
}