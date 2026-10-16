using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Samples.UniversalInterceptor;

[CompileTime]
internal static class DiagnosticDefinitions
{
    public static readonly DiagnosticDefinition<INamedType> InterceptorTypeNotImplemented = new(
        "UNIV01",
        Severity.Error,
        "The type '{0}' cannot give the universal interceptor because it does not implement IUniversalInterceptor." );

    public static readonly DiagnosticDefinition<(INamedType Type, string Member)> InterceptorMemberNotFound = new(
        "UNIV02",
        Severity.Error,
        "The type '{0}' has no field or property named '{1}'." );

    public static readonly DiagnosticDefinition<IFieldOrProperty> InterceptorMemberNotStatic = new(
        "UNIV03",
        Severity.Error,
        "The member '{0}' cannot give the universal interceptor because it is not static." );

    public static readonly DiagnosticDefinition<(IFieldOrProperty Member, IType Type)> InterceptorMemberTypeInvalid =
        new(
            "UNIV04",
            Severity.Error,
            "The member '{0}' cannot give the universal interceptor because its type is '{1}'. "
            + "The type must be IUniversalInterceptor or AsyncLocal<IUniversalInterceptor>." );

    public static readonly DiagnosticDefinition<(IMethod Method, string Reason)> CallNotIntercepted = new(
        "UNIV05",
        Severity.Warning,
        "The call to '{0}' is not intercepted because {1}. To remove this warning, skip the call site in ForEachSite." );
}
