using Metalama.Samples.UniversalInterceptor;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor.UnitTests;

// An interceptor that returns canned results instead of calling the weather service, and records the intercepted
// calls. The calls to a method without a canned result proceed to the weather client.
internal sealed class FakeWeatherInterceptor : IUniversalInterceptor
{
    private readonly Dictionary<string, object?> _results = new();
    private readonly ConcurrentQueue<string> _calls = new();

    public IReadOnlyCollection<string> Calls => this._calls;

    public FakeWeatherInterceptor Returns( string methodName, object? result )
    {
        this._results[methodName] = result;

        return this;
    }

    public FakeWeatherInterceptor Throws( string methodName, Exception exception )
    {
        this._results[methodName] = exception;

        return this;
    }

    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple
        => this.TryGetResult<TResult>( metadata, out var result ) ? result : proceed( ref args );

    public Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed )
        where TArgs : struct, ITuple
        => this.TryGetResult<TResult>( metadata, out var result ) ? Task.FromResult( result ) : proceed( args );

    public ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed )
        where TArgs : struct, ITuple
        => this.TryGetResult<TResult>( metadata, out var result ) ? ValueTask.FromResult( result ) : proceed( args );

    private bool TryGetResult<TResult>( InterceptionMetadata metadata, out TResult result )
    {
        this._calls.Enqueue( metadata.MethodName );

        if ( !this._results.TryGetValue( metadata.MethodName, out var value ) )
        {
            result = default!;

            return false;
        }

        if ( value is Exception exception )
        {
            throw exception;
        }

        result = (TResult) value!;

        return true;
    }
}
