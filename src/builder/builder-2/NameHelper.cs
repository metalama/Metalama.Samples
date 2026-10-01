using Metalama.Framework.Aspects;

namespace Metalama.Samples.Builder2;

[CompileTime]
internal static class NameHelper
{
    public static string ToParameterName( string name ) => name[0].ToString().ToLowerInvariant() + name[1..];
}