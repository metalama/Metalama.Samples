using Metalama.Framework.Aspects;

namespace Metalama.Samples.Builder3;

[CompileTime]
internal static class NameHelper
{
    public static string ToParameterName( string name ) => name[0].ToString().ToLowerInvariant() + name[1..];

    public static string ToFieldName( string name ) => $"_{name[0].ToString().ToLowerInvariant()}{name[1..]}";
}