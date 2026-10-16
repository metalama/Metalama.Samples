using Metalama.Extensions.Interceptors;
using Metalama.Framework.Fabrics;
using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor.Tests.StructReceiverWarning;

// Without ForEachSite, RedirectToUniversalInterceptor reports a warning for the call on the RequestQuota struct, and
// leaves the call unchanged.
internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.InterceptMethods(
            calls => calls.Type( typeof(RequestQuota) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(PassThroughInterceptor), nameof(PassThroughInterceptor.Current) ) );
}

public class PassThroughInterceptor : IUniversalInterceptor
{
    public static readonly IUniversalInterceptor Current = new PassThroughInterceptor();

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

// <target>
public class Sender
{
    public bool TrySend( RequestQuota quota ) => quota.TryConsume();
}
