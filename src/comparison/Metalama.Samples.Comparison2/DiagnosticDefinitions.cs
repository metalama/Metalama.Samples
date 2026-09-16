using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Comparison2;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<(IMethod BaseMethod, INamedType CurrentType)>
        BaseMethodMustBeVirtual
            = new(
                "EQU001",
                Severity.Error,
                "The '{0}' method must be virtual and non-sealed because it must be overridden by the '{1}' type." );
}