using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder2;

[Inheritable]
public partial class GenerateBuilderAttribute : TypeAspect
{
    [CompileTime]
    private record Tags(
        INamedType SourceType,
        IReadOnlyList<PropertyMapping> Properties,
        IConstructor SourceConstructor,
        IConstructor BuilderCopyConstructor );

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var hasError = false;

        var sourceType = builder.Target;

        // [<snippet FindBaseType>]
        // Find the Builder nested type in the base type.
        INamedType? baseBuilderType = null;

        IConstructor? baseConstructor = null,
                      baseBuilderConstructor = null,
                      baseBuilderCopyConstructor = null;

        if ( sourceType.BaseType != null && sourceType.BaseType.SpecialType != SpecialType.Object )
        {
            // We need to filter parameters to work around a bug where the Constructors collection
            // contains the implicit constructor.
            var baseTypeConstructors =
                sourceType.BaseType.Constructors.Where( c => c.Parameters.Count > 0 ).ToList();

            if ( baseTypeConstructors.Count != 1 )
            {
                builder.Diagnostics.Report(
                    BuilderDiagnosticDefinitions.BaseTypeMustContainOneConstructor.WithArguments(
                        (
                            sourceType.BaseType, baseTypeConstructors.Count) ) );

                hasError = true;
            }
            else
            {
                baseConstructor = baseTypeConstructors[0];
            }

            var baseBuilderTypes =
                sourceType.BaseType.Types.OfName( "Builder" ).ToList();

            switch ( baseBuilderTypes.Count )
            {
                case 0:
                    builder.Diagnostics.Report(
                        BuilderDiagnosticDefinitions.BaseTypeMustContainABuilderType.WithArguments(
                            sourceType.BaseType ) );

                    return;

                case > 1:
                    builder.Diagnostics.Report(
                        BuilderDiagnosticDefinitions.BaseTypeCannotContainMoreThanOneBuilderType
                            .WithArguments( sourceType.BaseType ) );

                    return;

                default:
                    baseBuilderType = baseBuilderTypes[0];

                    // Check that we have exactly two constructors.
                    if ( baseBuilderType.Constructors.Count != 2 )
                    {
                        builder.Diagnostics.Report(
                            BuilderDiagnosticDefinitions.BaseBuilderMustContainOneNonCopyConstructor
                                .WithArguments(
                                    (baseBuilderType,
                                     baseBuilderType.Constructors.Count) ) );

                        return;
                    }

                    // Find the copy constructor.
                    baseBuilderCopyConstructor = baseBuilderType.Constructors
                        .SingleOrDefault( c =>
                                              c.Parameters.Count == 1 &&
                                              c.Parameters[0].Type.Equals( sourceType.BaseType ) );

                    if ( baseBuilderCopyConstructor == null )
                    {
                        builder.Diagnostics.Report(
                            BuilderDiagnosticDefinitions.BaseBuilderMustContainCopyConstructor
                                .WithArguments(
                                    (baseBuilderType,
                                     sourceType.BaseType) ) );

                        return;
                    }

                    // The normal constructor is the other constructor.
                    baseBuilderConstructor =
                        baseBuilderType.Constructors.Single( c => c != baseBuilderCopyConstructor );

                    break;
            }
        }

        if ( hasError )
        {
            return;
        }

        // [<endsnippet FindBaseType>]

        // [<snippet CreatePropertyMap>]
        // Create a list of PropertyMapping items for all properties that we want to build using the Builder.
        var properties = sourceType.AllProperties.Where( p => p.Writeability != Writeability.None &&
                                                              !p.IsStatic )
            .Select( p =>
            {
                var isRequired = p.Attributes.OfAttributeType( typeof(RequiredAttribute) )
                    .Any();

                var isInherited = !p.DeclaringType.Equals( sourceType );

                return new PropertyMapping( p, isRequired, isInherited );
            } )
            .ToList();

        // [<endsnippet CreatePropertyMap>]

        // [<snippet IntroduceBuilderType>]
        // Introduce the Builder nested type.
        var builderType = builder.IntroduceClass(
            "Builder",
            OverrideStrategy.New,
            t =>
            {
                t.Accessibility = Accessibility.Public;
                t.BaseType = baseBuilderType;
                t.IsSealed = sourceType.IsSealed;
            } );

        // [<endsnippet IntroduceBuilderType>]

        // [<snippet CreateProperties>]
        // Add builder properties and update the mapping.  
        foreach ( var property in properties )
        {
            if ( property.IsInherited )
            {
                // For properties of the base type, find the matching property.
                var baseProperty =
                    baseBuilderType!.AllProperties.OfName( property.SourceProperty.Name )
                        .SingleOrDefault();

                if ( baseProperty == null )
                {
                    builder.Diagnostics.Report(
                        BuilderDiagnosticDefinitions.BaseBuilderMustContainProperty.WithArguments(
                            (
                                baseBuilderType, property.SourceProperty.Name) ) );

                    hasError = true;
                }
                else
                {
                    property.BuilderProperty = baseProperty;
                }
            }
            else
            {
                // For properties of the current type, introduce a new property.
                property.BuilderProperty =
                    builderType.IntroduceAutomaticProperty(
                            property.SourceProperty.Name,
                            property.SourceProperty.Type,
                            IntroductionScope.Instance,
                            buildProperty: p =>
                            {
                                p.Accessibility = Accessibility.Public;

                                p.InitializerExpression =
                                    property.SourceProperty.InitializerExpression;
                            } )
                        .Declaration;
            }
        }

        // [<endsnippet CreateProperties>]

        if ( hasError )
        {
            return;
        }

        // [<snippet CreateBuilderConstructor>]
        // Add a builder constructor accepting the required properties and update the mapping.
        builderType.IntroduceConstructor(
            nameof(this.BuilderConstructorTemplate),
            buildConstructor: c =>
            {
                c.Accessibility = Accessibility.Public;

                // Adding parameters.
                foreach ( var property in properties.Where( m => m.IsRequired ) )
                {
                    var parameter = c.AddParameter(
                        NameHelper.ToParameterName( property.SourceProperty.Name ),
                        property.SourceProperty.Type );

                    property.BuilderConstructorParameterIndex = parameter.Index;
                }

                // Calling the base constructor.
                if ( baseBuilderConstructor != null )
                {
                    c.InitializerKind = ConstructorInitializerKind.Base;

                    foreach ( var baseConstructorParameter in baseBuilderConstructor.Parameters )
                    {
                        var thisParameter =
                            c.Parameters.SingleOrDefault( p =>
                                                              p.Name == baseConstructorParameter.Name );

                        if ( thisParameter != null )
                        {
                            c.AddInitializerArgument( thisParameter );
                        }
                        else
                        {
                            builder.Diagnostics.Report(
                                BuilderDiagnosticDefinitions
                                    .BaseTypeConstructorHasUnexpectedParameter.WithArguments(
                                        (
                                            baseBuilderConstructor,
                                            baseConstructorParameter.Name) ) );

                            hasError = true;
                        }
                    }
                }
            } );

        // [<endsnippet CreateBuilderConstructor>]

        // Add a builder constructor that creates a copy from the source type.
        var builderCopyConstructor = builderType.IntroduceConstructor(
                nameof(this.BuilderCopyConstructorTemplate),
                buildConstructor: c =>
                {
                    c.Accessibility = Accessibility.ProtectedInternal;
                    c.Parameters[0].Type = sourceType;

                    if ( baseBuilderCopyConstructor != null )
                    {
                        c.InitializerKind = ConstructorInitializerKind.Base;

                        if ( baseBuilderType != null )
                        {
                            c.AddInitializerArgument( c.Parameters[0] );
                        }
                    }
                } )
            .Declaration;

        if ( hasError )
        {
            return;
        }

        // Add a Build method to the builder.
        builderType.IntroduceMethod(
            nameof(this.BuildMethodTemplate),
            IntroductionScope.Instance,
            OverrideStrategy.New,
            m =>
            {
                m.Name = "Build";
                m.Accessibility = Accessibility.Public;
                m.ReturnType = sourceType;
            } );

        // Add a constructor to the source type with all properties.
        var sourceConstructor = builder.IntroduceConstructor(
                nameof(this.SourceConstructorTemplate),
                buildConstructor: c =>
                {
                    c.Accessibility = sourceType.IsSealed
                        ? Accessibility.Private
                        : Accessibility.Protected;

                    foreach ( var property in properties )
                    {
                        var parameter = c.AddParameter(
                            NameHelper.ToParameterName( property.SourceProperty.Name ),
                            property.SourceProperty.Type );

                        property.SourceConstructorParameterIndex = parameter.Index;
                    }

                    if ( baseConstructor != null )
                    {
                        c.InitializerKind = ConstructorInitializerKind.Base;

                        foreach ( var baseConstructorParameter in baseConstructor.Parameters )
                        {
                            var thisParameter = c.Parameters.SingleOrDefault( p =>
                                                                                  p.Name == baseConstructorParameter
                                                                                      .Name );

                            if ( thisParameter == null )
                            {
                                builder.Diagnostics.Report(
                                    BuilderDiagnosticDefinitions
                                        .BaseTypeConstructorHasUnexpectedParameter.WithArguments(
                                            (
                                                baseConstructor, baseConstructorParameter.Name) ) );

                                hasError = true;
                            }
                            else
                            {
                                c.AddInitializerArgument( thisParameter );
                            }
                        }
                    }
                } )
            .Declaration;

        // Add a ToBuilder method to the source type.
        builder.IntroduceMethod(
            nameof(this.ToBuilderMethodTemplate),
            whenExists: OverrideStrategy.Override,
            buildMethod: m =>
            {
                m.Accessibility = Accessibility.Public;
                m.Name = "ToBuilder";
                m.ReturnType = builderType.Declaration;
                m.IsVirtual = !sourceType.IsSealed;
            } );

        builder.Tags = new Tags(
            builder.Target,
            properties,
            sourceConstructor,
            builderCopyConstructor );
    }

    [Template]
    private void BuilderConstructorTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties.Where( p => p is
                                                             { IsRequired: true, IsInherited: false } ) )
        {
            property.BuilderProperty!.Value =
                meta.Target.Parameters[property.BuilderConstructorParameterIndex!.Value].Value;
        }
    }

    [Template]
    private void BuilderCopyConstructorTemplate( dynamic source )
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties.Where( p => !p.IsInherited ) )
        {
            property.BuilderProperty!.Value =
                property.SourceProperty.WithObject( (IExpression) source ).Value;
        }
    }

    [Template]
    private void SourceConstructorTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        foreach ( var property in tags.Properties.Where( p => !p.IsInherited ) )
        {
            property.SourceProperty.Value =
                meta.Target.Parameters[property.SourceConstructorParameterIndex!.Value].Value;
        }
    }

    [Template]
    private dynamic BuildMethodTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        // Build the object.
        var instance =
            tags.SourceConstructor.Invoke( tags.Properties.Select( x => x.BuilderProperty! ) )!;

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

    [Template]
    private dynamic ToBuilderMethodTemplate()
    {
        var tags = (Tags) meta.Tags.Source!;

        return tags.BuilderCopyConstructor.Invoke( meta.This );
    }
}