using Metalama.Extensions.Architecture.Aspects;
using Metalama.Extensions.Architecture.Fabrics;
using Metalama.Extensions.Architecture.Predicates;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

public class SingletonAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Outbound
            .SelectMany( type => type.Constructors )
            .AddAspect( _ => new CanOnlyBeUsedFromAttribute { Namespaces = new[] { "**.Tests" } } );
    }
}