using Metalama.Samples.Proxy;
using Proxies;
using Xunit;

namespace Metalama.Samples.Proxy.UnitTests;

// The CalculatorProxy class is generated at compile time by GenerateProxyAspect from the ICalculator
// interface. These tests execute the generated code.
public sealed class CalculatorProxyTests
{
    private static (ICalculator Proxy, Calculator Intercepted, StringWriter Log) CreateProxy()
    {
        var log = new StringWriter();
        var intercepted = new Calculator();
        var proxy = new CalculatorProxy( new LoggingInterceptor( log ), intercepted );

        return (proxy, intercepted, log);
    }

    [Fact]
    public void GeneratedProxyImplementsTheInterface()
    {
        var (proxy, _, _) = CreateProxy();

        Assert.IsAssignableFrom<ICalculator>( proxy );
    }

    [Fact]
    public void ReturnValueFlowsBackThroughTheInterceptor()
    {
        var (proxy, _, _) = CreateProxy();

        Assert.Equal( 5, proxy.Add( 2, 3 ) );
    }

    [Fact]
    public void ArgumentsReachTheInterceptedObject()
    {
        var (proxy, intercepted, _) = CreateProxy();

        proxy.Store( "x", 42 );

        Assert.Equal( 42, Assert.Contains( "x", intercepted.Memory ) );
    }

    [Fact]
    public void MethodWithoutParameterIsIntercepted()
    {
        var (proxy, intercepted, log) = CreateProxy();

        proxy.Store( "x", 42 );
        proxy.Clear();

        Assert.Empty( intercepted.Memory );
        Assert.Contains( "Clear() started.", log.ToString(), StringComparison.Ordinal );
    }

    [Fact]
    public void InterceptorSeesTheMethodNameAndArguments()
    {
        var (proxy, _, log) = CreateProxy();

        proxy.Add( 2, 3 );

        var lines = log.ToString().Split( Environment.NewLine, StringSplitOptions.RemoveEmptyEntries );

        Assert.Equal( "Add(2, 3) started.", lines[0] );
        Assert.Equal( "Add(2, 3) returned 5.", lines[1] );
    }

    [Fact]
    public void VoidMethodIsReportedAsReturningVoid()
    {
        var (proxy, _, log) = CreateProxy();

        proxy.Store( "x", 42 );

        Assert.Contains( "Store(x, 42) returned void.", log.ToString(), StringComparison.Ordinal );
    }

    [Fact]
    public void ExceptionPropagatesAndIsReported()
    {
        var log = new StringWriter();
        var proxy = new CalculatorProxy( new LoggingInterceptor( log ), new ThrowingCalculator() );

        Assert.Throws<InvalidOperationException>( () => proxy.Add( 2, 3 ) );
        Assert.Contains( "Add(2, 3) failed: InvalidOperationException.", log.ToString(), StringComparison.Ordinal );
    }

    private sealed class ThrowingCalculator : ICalculator
    {
        public int Add( int a, int b ) => throw new InvalidOperationException();

        public void Store( string name, int value ) => throw new InvalidOperationException();

        public void Clear() => throw new InvalidOperationException();
    }
}
