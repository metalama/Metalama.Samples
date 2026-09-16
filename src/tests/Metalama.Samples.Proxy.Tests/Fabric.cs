using Metalama.Framework.Fabrics;

namespace Metalama.Samples.Proxy.Tests.Fabric;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectReflectionType( typeof(ISomeInterface) ).GenerateStaticProxy();
    }
}

public interface ISomeInterface
{
    void VoidMethod( int a, string b );

    int NonVoidMethod( int a, string b );

    void VoidNoParamMethod();
}