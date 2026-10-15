using Metalama.Extensions.Interceptors;
using Metalama.Framework.Fabrics;
using Metalama.Samples.UniversalInterceptor;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor1.Tests.InvalidInterceptorMember;

// RedirectToUniversalInterceptor reports an error at each call site when the member that gives the interceptor is not
// valid, and the call is not intercepted.
internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // The type does not implement IUniversalInterceptor.
        amender.InterceptMethods(
            calls => calls.Type( typeof(ServiceA) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(NotAnInterceptor), nameof(NotAnInterceptor.Current) ) );

        // The member does not exist.
        amender.InterceptMethods(
            calls => calls.Type( typeof(ServiceB) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(Interceptor), "Missing" ) );

        // The member is not static.
        amender.InterceptMethods(
            calls => calls.Type( typeof(ServiceC) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(Interceptor), nameof(Interceptor.InstanceMember) ) );

        // The member has another type.
        amender.InterceptMethods(
            calls => calls.Type( typeof(ServiceD) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(Interceptor), nameof(Interceptor.WrongType) ) );
    }
}

public class NotAnInterceptor
{
    public static readonly NotAnInterceptor Current = new();
}

public class Interceptor : IUniversalInterceptor
{
    public static readonly Interceptor WrongType = new();

    public IUniversalInterceptor InstanceMember => this;

    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple
        => proceed( ref args );

    public Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed )
        where TArgs : struct, ITuple
        => proceed( args );

    public ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed )
        where TArgs : struct, ITuple
        => proceed( args );
}

public class ServiceA
{
    public void Run() { }
}

public class ServiceB
{
    public void Run() { }
}

public class ServiceC
{
    public void Run() { }
}

public class ServiceD
{
    public void Run() { }
}

// <target>
public class Program
{
    public void Execute()
    {
        new ServiceA().Run();
        new ServiceB().Run();
        new ServiceC().Run();
        new ServiceD().Run();
    }
}
