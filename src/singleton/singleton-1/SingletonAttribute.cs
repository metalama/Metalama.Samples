using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

public class SingletonAttribute : TypeAspect
{
    // [<snippet InstanceTemplate>]
    [Template]
    public static object Instance { get; }

    // [<endsnippet InstanceTemplate>]

    // [<snippet PrivateConstructorDiagnostic>]
    private static readonly DiagnosticDefinition<(IConstructor, INamedType)>
        _constructorHasToBePrivate = new(
            "SING01",
            Severity.Warning,
            "The '{0}' constructor must be private because the class is [Singleton]." );

    // [<endsnippet PrivateConstructorDiagnostic>]

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // [<snippet IntroduceInstanceProperty>]
        // Introduce the property.
        builder.IntroduceProperty(
            nameof(Instance),
            buildProperty: propertyBuilder =>
            {
                propertyBuilder.Type = builder.Target;

                var initializer = new ExpressionBuilder();
                initializer.AppendVerbatim( "new " );
                initializer.AppendTypeName( builder.Target );
                initializer.AppendVerbatim( "()" );

                propertyBuilder.InitializerExpression = initializer.ToExpression();
            } );

        // [<endsnippet IntroduceInstanceProperty>]

        // [<snippet PrivateConstructorReport>]
        // Verify constructors.
        foreach ( var constructor in builder.Target.Constructors )
        {
            if ( constructor.Accessibility != Accessibility.Private &&
                 !constructor.IsImplicitlyDeclared )
            {
                builder.Diagnostics.Report(
                    _constructorHasToBePrivate.WithArguments( (constructor, builder.Target) ),
                    constructor );
            }
        }

        // [<endsnippet PrivateConstructorReport>]

        // [<snippet AddPrivateConstructor>]
        // If there is no explicit constructor, add one.
        if ( builder.Target.Constructors.All( c =>
                                                  c.IsImplicitlyDeclared ) )
        {
            builder.IntroduceConstructor(
                nameof(this.ConstructorTemplate),
                buildConstructor: c => c.Accessibility = Accessibility.Private );
        }

        // [<endsnippet AddPrivateConstructor>]
    }

    [Template]
    private void ConstructorTemplate() { }

    public override void BuildEligibility( IEligibilityBuilder<INamedType> builder )
        => builder.MustSatisfy(
            t => t.TypeKind is TypeKind.Class,
            t => $"{t} must be a class" );
}