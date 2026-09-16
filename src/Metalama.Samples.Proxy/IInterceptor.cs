using System.Runtime.CompilerServices;

namespace Metalama.Samples.Proxy;

public interface IInterceptor
{
    TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed ) where TArgs : struct, ITuple;

    Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed ) where TArgs : struct, ITuple;

    ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed ) where TArgs : struct, ITuple;
}