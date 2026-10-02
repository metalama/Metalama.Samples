using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

public class SingletonAttribute : TypeAspect
{
    [Template]
    public static object Instance { get; }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var instanceProperty = builder.Advice.IntroduceProperty( builder.Target, nameof(Instance), buildProperty: propertyBuilder =>
        {
            propertyBuilder.Type = builder.Target;

            var initializer = new ExpressionBuilder();
            initializer.AppendVerbatim( "new " );
            initializer.AppendTypeName( builder.Target );
            initializer.AppendVerbatim( "()" );

            propertyBuilder.InitializerExpression = initializer.ToExpression();
        } );
    }
}