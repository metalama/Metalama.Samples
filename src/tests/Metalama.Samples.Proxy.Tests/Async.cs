using Metalama.Samples.Proxy;
using Metalama.Samples.Proxy.Tests.Async;

[assembly: GenerateProxyAspect( typeof(ISomeInterface), "SomeProxy", "Metalama.Samples.Proxy.Tests" )]

namespace Metalama.Samples.Proxy.Tests.Async;

public interface ISomeInterface
{
    Task TaskMethodAsync( int a, string b );

    Task<int> TaskOfIntMethodAsync( int a, string b );

    ValueTask ValueTaskMethodAsync( int a, string b );

    ValueTask<int> ValueTaskOfIntMethodAsync( int a, string b );
}