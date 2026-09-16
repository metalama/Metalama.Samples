using Metalama.Samples.Proxy;
using Metalama.Samples.Proxy.Tests.OutRefParameters;

[assembly: GenerateProxyAspect( typeof(ISomeInterface), "SomeProxy", "Metalama.Samples.Proxy.Tests" )]

namespace Metalama.Samples.Proxy.Tests.OutRefParameters;

public interface ISomeInterface
{
    void VoidMethod( out int a, ref string b, in DateTime dt, ref readonly TimeSpan ts );

    int NonVoidMethod( out int a, ref string b, in DateTime dt, ref readonly TimeSpan ts );
}