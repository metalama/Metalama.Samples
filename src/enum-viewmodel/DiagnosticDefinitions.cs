using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<INamedType> NotAnEnumError =
        new(
            "ENUM01",
            Severity.Error,
            "The type '{0}' is not an enum." );
}