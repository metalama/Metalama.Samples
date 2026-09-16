using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Comparison4;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<(IMethod BaseMethod, INamedType CurrentType)>
        BaseMethodMustBeVirtual
            = new(
                "EQU001",
                Severity.Error,
                "The '{0}' method must be virtual and non-sealed because it must be overridden by the '{1}' type." );

    public static readonly DiagnosticDefinition<INamedType>
        NoEqualityMemberError
            = new(
                "EQU002",
                Severity.Error,
                "The type '{0}' does not have any field or property annotated with the [EqualityMember] attribute." );
}