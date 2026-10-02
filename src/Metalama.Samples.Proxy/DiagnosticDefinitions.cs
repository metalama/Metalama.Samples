using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.Proxy;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<(INamedType, IMethod)> AwaitableTypeNotSupported = new(
        "PROXY01",
        Severity.Error,
        "Cannot generate a proxy for '{0}' because the return type of method '{1}' is awaitable but is not a Task or ValueTask. ",
        "Cannot generate a proxy for the type because it contains a method with awaitable return type that is not a Task or ValueTask." );
}