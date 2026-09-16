using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder1;

// [<snippet ClassHeader>]
public partial class GenerateBuilderAttribute : TypeAspect

// [<endsnippet ClassHeader>]
{
    // [<snippet InitializeMapping>]
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var sourceType = builder.Target;

        // Create a list of PropertyMapping items for all properties that we want to build using the Builder.
        var properties = sourceType.Properties.Where( p => p.Writeability != Writeability.None &&
                                                           !p.IsStatic )
            .Select( p => new PropertyMapping(
                         p,
                         p.Attributes.OfAttributeType( typeof(RequiredAttribute) ).Any() ) )
            .ToList();

        // [<endsnippet InitializeMapping>]

        // [<snippet IntroduceBuilder>]
        // Introduce the Builder nested type.
        var builderType = builder.IntroduceClass(
            "Builder",
            buildType: t => t.Accessibility = Accessibility.Public );

        // [<endsnippet IntroduceBuilder>]

        // [<snippet IntroduceProperties>]
        // Add builder properties and update the mapping.
        foreach ( var property in properties )
        {
            property.BuilderProperty =
                builderType.IntroduceAutomaticProperty(
                        property.SourceProperty.Name,
                        property.SourceProperty.Type,
                        buildProperty: p =>
                        {
                            p.Accessibility = Accessibility.Public;
                            p.InitializerExpression = property.SourceProperty.InitializerExpression;
                        } )
                    .Declaration;
        }

        // [<endsnippet IntroduceProperties>]

        // [<snippet IntroducePublicConstructor>]
        // Add a builder constructor accepting the required properties and update the mapping.
        builderType.IntroduceConstructor(
            nameof(this.BuilderConstructorTemplate),
            buildConstructor: c =>
            {
                c.Accessibility = Accessibility.Public;

                foreach ( var property in properties.Where( m => m.IsRequired ) )
                {
                    var parameter = c.AddParameter(
                        NameHelper.ToParameterName( property.SourceProperty.Name ),
                        property.SourceProperty.Type );

                    property.BuilderConstructorParameterIndex = parameter.Index;
                }
            } );

        // [<endsnippet IntroducePublicConstructor>]

        // [<snippet IntroduceSourceConstructor>]
        // Add a constructor to the source type with all properties.
        var sourceConstructor = builder.IntroduceConstructor(
                nameof(this.SourceConstructorTemplate),
                buildConstructor: c =>
                {
                    c.Accessibility = Accessibility.Private;

                    foreach ( var property in properties )
                    {
                        var parameter = c.AddParameter(
                            NameHelper.ToParameterName( property.SourceProperty.Name ),
                            property.SourceProperty.Type );

                        property.SourceConstructorParameterIndex = parameter.Index;
                    }
                } )
            .Declaration;

        // [<endsnippet IntroduceSourceConstructor>]

        // [<snippet IntroduceBuildMethod>]
        // Add a Build method to the builder.
        builderType.IntroduceMethod(
            nameof(this.BuildMethodTemplate),
            IntroductionScope.Instance,
            buildMethod: m =>
            {
                m.Name = "Build";
                m.Accessibility = Accessibility.Public;
                m.ReturnType = sourceType;
            } );

        // [<endsnippet IntroduceBuildMethod>]

        // [<snippet IntroduceCopyConstructor>]
        // Add a builder constructor that creates a copy from the source type.
        var builderCopyConstructor = builderType.IntroduceConstructor(
                nameof(this.BuilderCopyConstructorTemplate),
                buildConstructor: c =>
                {
                    c.Accessibility = Accessibility.Internal;
                    c.Parameters[0].Type = sourceType;
                } )
            .Declaration;

        // [<endsnippet IntroduceCopyConstructor>]

        // [<snippet IntroduceToBuilderMethod>]
        // Add a ToBuilder method to the source type.
        builder.IntroduceMethod(
            nameof(this.ToBuilderMethodTemplate),
            buildMethod: m =>
            {
                m.Accessibility = Accessibility.Public;
                m.Name = "ToBuilder";
                m.ReturnType = builderType.Declaration;
            } );

        // [<endsnippet IntroduceToBuilderMethod>]

        // [<snippet SetTags>]
        builder.Tags = new Tags(
            builder.Target,
            properties,
            sourceConstructor,
            builderCopyConstructor );

        // [<endsnippet SetTags>]
    }

    // [<snippet BuilderConstructorTemplate>]
    [Template]
    private void BuilderConstructorTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties.Where( p => p.IsRequired ) )
        {
            property.BuilderProperty!.Value =
                meta.Target.Parameters[property.BuilderConstructorParameterIndex!.Value].Value;
        }
    }

    // [<endsnippet BuilderConstructorTemplate>]

    [Template]
    private void BuilderCopyConstructorTemplate( dynamic source )
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties )
        {
            property.BuilderProperty!.Value =
                property.SourceProperty.WithObject( (IExpression) source ).Value;
        }
    }

    // [<snippet SourceConstructorTemplate>]
    [Template]
    private void SourceConstructorTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties )
        {
            property.SourceProperty.Value =
                meta.Target.Parameters[property.SourceConstructorParameterIndex!.Value].Value;
        }
    }

    // [<endsnippet SourceConstructorTemplate>]

    // [<snippet BuildMethodTemplate>]
    [Template]
    private dynamic BuildMethodTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        // Build the object.
        var instance = tags.SourceConstructor.Invoke( tags.Properties.Select( x => x.BuilderProperty! ) )!;

        // Find and invoke the Validate method, if any.
        var validateMethod = tags.SourceType.AllMethods.OfName( "Validate" )
            .SingleOrDefault( m => m.Parameters.Count == 0 );

        if ( validateMethod != null )
        {
            validateMethod.WithObject( (IExpression) instance ).Invoke();
        }

        // Return the object.
        return instance;
    }

    // [<endsnippet BuildMethodTemplate>]

    [Template]
    private dynamic ToBuilderMethodTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        return tags.BuilderCopyConstructor.Invoke( meta.This );
    }
}