using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Metalama.Samples.Proxy;

[CompileTime]
public static class FabricExtensions
{
    public static void GenerateStaticProxy(
        this IQuery<INamedType> receiver,
        Func<INamedType, string>? getProxyTypeName,
        Func<INamedType, string>? getProxyNamespace = null )
    {
        receiver.WithTag( type => type )
            .Select( type => type.Compilation )
            .AddAspect( ( _, type ) => new GenerateProxyAspect(
                            type,
                            getProxyTypeName?.Invoke( type ) ?? type.Name.Substring( 1 ) + "Proxy",
                            getProxyNamespace?.Invoke( type ) ?? type.ContainingNamespace.FullName ) );
    }

    public static void GenerateStaticProxy(
        this IQuery<INamedType> receiver,
        string? proxyTypeName = null,
        string? proxyNamespace = null )
    {
        receiver.WithTag( type => type )
            .Select( type => type.Compilation )
            .AddAspect( ( _, type ) => new GenerateProxyAspect(
                            type,
                            proxyTypeName ?? type.Name.Substring( 1 ) + "Proxy",
                            proxyNamespace ?? type.ContainingNamespace.FullName ) );
    }
}