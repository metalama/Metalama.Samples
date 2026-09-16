using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace DefaultNamespace;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static DiagnosticDefinition<INamedType> BaseTypeHasNoMementoType
        = new(
            "MEMENTO01",
            Severity.Error,
            "The base type '{0}' does not have a 'Memento' nested type." );

    public static DiagnosticDefinition<INamedType> MementoTypeMustBeProtected
        = new(
            "MEMENTO02",
            Severity.Error,
            "The type '{0}' must be protected." );

    public static DiagnosticDefinition<INamedType> MementoTypeMustNotBeSealed
        = new(
            "MEMENTO03",
            Severity.Error,
            "The type '{0}' must be not be sealed." );

    public static DiagnosticDefinition<(INamedType MementoType, IType ParameterType)>
        MementoTypeMustHaveConstructor
            = new(
                "MEMENTO04",
                Severity.Error,
                "The type '{0}' must have a constructor with a single parameter of type '{1}'." );

    public static DiagnosticDefinition<IConstructor> MementoConstructorMustBePublicOrProtected
        = new(
            "MEMENTO05",
            Severity.Error,
            "The constructor '{0}' must be public or protected." );
}