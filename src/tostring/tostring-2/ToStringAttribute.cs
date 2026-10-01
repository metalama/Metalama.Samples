using Metalama.Extensions.CodeFixes;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

[EditorExperience( SuggestAsLiveTemplate = true )]
public class ToStringAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        // [<snippet AddNotToStringCodeFix>]
        // For each property, suggest a code fix to remove from ToString.
        foreach ( var property in GetIncludedProperties( builder.Target ) )
        {
            builder.Diagnostics.Suggest(
                CodeFixFactory.AddAttribute(
                    property,
                    typeof(NotToStringAttribute),
                    "Exclude from [ToString]" ),
                property );
        }

        // [<endsnippet AddNotToStringCodeFix>]

        // [<snippet SwitchToManualImplementation>]
        // Suggest to switch to manual implementation.
        if ( builder.AspectInstance.Predecessors[0].Instance is IAttribute attribute )
        {
            builder.Diagnostics.Suggest(
                new CodeFix(
                    "Switch to manual implementation",
                    async codeFixBuilder =>
                    {
                        await codeFixBuilder.ApplyAspectAsync( builder.Target, this );

                        await codeFixBuilder.RemoveAttributesAsync(
                            builder.Target,
                            typeof(ToStringAttribute) );

                        await codeFixBuilder.RemoveAttributesAsync(
                            builder.Target,
                            typeof(NotToStringAttribute) );
                    } ),
                attribute );
        }

        // [<endsnippet SwitchToManualImplementation>]
    }

    [CompileTime]
    private static IEnumerable<IFieldOrProperty> GetIncludedProperties( INamedType target )
        => target.AllFieldsAndProperties
            .Where( f => f is
            {
                IsStatic: false, IsImplicitlyDeclared: false, Accessibility: Accessibility.Public
            } )
            .Where( p => !p.Attributes.Any( typeof(NotToStringAttribute) ) );

    [Introduce( WhenExists = OverrideStrategy.Override, Name = "ToString" )]
    public string IntroducedToString()
    {
        var stringBuilder = new InterpolatedStringBuilder();
        stringBuilder.AddText( "{ " );
        stringBuilder.AddText( meta.Target.Type.Name );
        stringBuilder.AddText( " " );

        var properties = GetIncludedProperties( meta.Target.Type )
            .OrderBy( f => f.Name );

        var i = meta.CompileTime( 0 );

        foreach ( var property in properties )
        {
            if ( i > 0 )
            {
                stringBuilder.AddText( ", " );
            }

            stringBuilder.AddText( property.Name );
            stringBuilder.AddText( "=" );
            stringBuilder.AddExpression( property );

            i++;
        }

        stringBuilder.AddText( " }" );

        return stringBuilder.ToValue();
    }
}