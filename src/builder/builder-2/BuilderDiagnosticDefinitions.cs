using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Builder2;

[CompileTime]
internal static class BuilderDiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<INamedType>
        BaseTypeCannotContainMoreThanOneBuilderType
            = new(
                "BUILDER01",
                Severity.Error,
                "The type '{0}' cannot contain more than one nested type named 'Builder'.",
                "The base type cannot contain more than one nested type named 'Builder'." );

    public static readonly DiagnosticDefinition<INamedType> BaseTypeMustContainABuilderType
        = new(
            "BUILDER02",
            Severity.Error,
            "The type '{0}' must contain a 'Builder' nested type.",
            "The base type cannot contain more than one builder type." );

    public static readonly DiagnosticDefinition<(INamedType, string)> BaseBuilderMustContainProperty
        = new(
            "BUILDER03",
            Severity.Error,
            "The '{0}' type must contain a property named '{1}'.",
            "The base builder type must contain properties for all properties of the base built type." );

    public static readonly DiagnosticDefinition<(INamedType, int)> BaseTypeMustContainOneConstructor
        = new(
            "BUILDER04",
            Severity.Error,
            "The '{0}' type must contain a single constructor but has {1}.",
            "The base type must contain a single constructor." );

    public static readonly DiagnosticDefinition<(IConstructor, string)>
        BaseTypeConstructorHasUnexpectedParameter
            = new(
                "BUILDER05",
                Severity.Error,
                "The '{1}' parameter of '{0}' cannot be mapped to a property.",
                "A parameter of the base type cannot be mapped to a property." );

    public static readonly DiagnosticDefinition<(INamedType BuilderType, INamedType SourceType)>
        BaseBuilderMustContainCopyConstructor
            = new(
                "BUILDER06",
                Severity.Error,
                "The '{0}' type must contain a constructor, called the copy constructor, with a single parameter of type '{1}'.",
                "The base type must contain a copy constructor." );

    public static readonly DiagnosticDefinition<(INamedType, int)>
        BaseBuilderMustContainOneNonCopyConstructor
            = new(
                "BUILDER07",
                Severity.Error,
                "The '{0}' type must contain exactly two constructors but has {1}.",
                "The base builder type must contain exactly two constructors." );
}