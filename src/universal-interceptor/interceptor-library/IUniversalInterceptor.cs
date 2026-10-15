using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor;

// The run-time object that receives the intercepted calls. TArgs is a value tuple that contains the receiver, when the
// intercepted method has one, and the arguments of the call.
public interface IUniversalInterceptor
{
    // Intercepts a call to a synchronous method. The ref and out arguments are written back to the call site from args
    // after the call. For a void method, TResult is ValueTuple.
    TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple;

    // Intercepts a call to a method that returns a Task or a Task<T>. For a Task, TResult is ValueTuple.
    Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed )
        where TArgs : struct, ITuple;

    // Intercepts a call to a method that returns a ValueTask or a ValueTask<T>. For a ValueTask, TResult is ValueTuple.
    ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed )
        where TArgs : struct, ITuple;
}
