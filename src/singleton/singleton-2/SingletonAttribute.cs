using Metalama.Extensions.Architecture;
using Metalama.Extensions.Architecture.Predicates;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

public class SingletonAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
        => builder.Outbound.SelectMany( t => t.Constructors )
            .CanOnlyBeUsedFrom(
                scope => scope.Namespace( "**.Tests" )
                    .Or()
                    .Type( typeof(Startup) ),
                "The class is a [Singleton]." );
}