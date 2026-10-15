using Metalama.Samples.UniversalInterceptor;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor3;

// Writes each intercepted call to the console: the method, the arguments, and the result or the exception.
public class CallLog : IUniversalInterceptor
{
    // The instance that receives the intercepted calls in the current asynchronous flow. A value assigned in a flow is
    // visible in this flow and in the flows that it starts, but not in the other flows. The value must be assigned
    // before the first intercepted call, otherwise the call throws a NullReferenceException.
    public static readonly AsyncLocal<IUniversalInterceptor> Current = new();

    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple
    {
        var call = FormatCall( args, metadata );

        try
        {
            // The intercepted method writes its ref and out arguments to args.
            var result = proceed( ref args );

            Console.WriteLine( $"[call] {call} {FormatResult( result )}, with the arguments ({FormatArguments( args, metadata )}) after the call." );

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"[call] {call} threw {e.GetType().Name}: {e.Message}" );

            throw;
        }
    }

    public async Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed )
        where TArgs : struct, ITuple
    {
        var call = FormatCall( args, metadata );

        try
        {
            var result = await proceed( args );

            Console.WriteLine( $"[call] {call} {FormatResult( result )} asynchronously." );

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"[call] {call} threw {e.GetType().Name} asynchronously: {e.Message}" );

            throw;
        }
    }

    public async ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed )
        where TArgs : struct, ITuple
    {
        var call = FormatCall( args, metadata );

        try
        {
            var result = await proceed( args );

            Console.WriteLine( $"[call] {call} {FormatResult( result )} asynchronously." );

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"[call] {call} threw {e.GetType().Name} asynchronously: {e.Message}" );

            throw;
        }
    }

    private static string FormatCall<TArgs>( TArgs args, InterceptionMetadata metadata )
        where TArgs : struct, ITuple
        => $"{metadata.DeclaringType.Name}.{metadata.MethodName}({FormatArguments( args, metadata )})";

    // The result of a method that returns void, a Task or a ValueTask is a ValueTuple.
    private static string FormatResult<TResult>( TResult result )
        => typeof(TResult) == typeof(ValueTuple) ? "succeeded" : $"returned {Format( result )}";

    // Formats the elements of the tuple, except the receiver.
    private static string FormatArguments<TArgs>( TArgs args, InterceptionMetadata metadata )
        where TArgs : struct, ITuple
    {
        var arguments = new List<string>();

        for ( var i = metadata.HasReceiver ? 1 : 0; i < args.Length; i++ )
        {
            arguments.Add( Format( args[i] ) );
        }

        return string.Join( ", ", arguments );
    }

    private static string Format( object? value )
        => value switch
        {
            null => "null",
            string[] array => $"[{string.Join( ", ", array )}]",
            CancellationToken => "CancellationToken",
            _ => value.ToString() ?? ""
        };
}
