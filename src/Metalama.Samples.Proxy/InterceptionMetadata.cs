using System.Reflection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Proxy;

public sealed class InterceptionMetadata
{
    public MethodInfo Method { get; }

    public bool ReturnsAwaitable { get; }

    public InterceptionMetadata( MethodInfo method, bool returnsAwaitable )
    {
        this.Method = method;
        this.ReturnsAwaitable = returnsAwaitable;
    }
}

[CompileTime]
internal sealed record InterceptionMetadataInfo( IMethod Method, IField MetadataField, bool ReturnsAwaitable );