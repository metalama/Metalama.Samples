using Metalama.Samples.UniversalInterceptor;
using System.Runtime.CompilerServices;

namespace Metalama.Samples.UniversalInterceptor1;

// Writes each intercepted call to the console: the method, the arguments, and the result or the exception.
public class CallLog : IUniversalInterceptor
{
    // The instance that receives the intercepted calls.
    public static readonly IUniversalInterceptor Current = new CallLog();

    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple
    {
        var call = $"{metadata.DeclaringType.Name}.{metadata.MethodName}({FormatArguments( args, metadata )})";

        try
        {
            var result = proceed( ref args );

            // The result of a void method is a ValueTuple.
            Console.WriteLine(
                typeof(TResult) == typeof(ValueTuple)
                    ? $"[call] {call} succeeded."
                    : $"[call] {call} returned {result}." );

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"[call] {call} threw {e.GetType().Name}: {e.Message}" );

            throw;
        }
    }

    // The asynchronous methods are logged in step 2. In this step, the calls proceed without logging.
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

    // Formats the elements of the tuple, except the receiver.
    private static string FormatArguments<TArgs>( TArgs args, InterceptionMetadata metadata )
        where TArgs : struct, ITuple
    {
        var arguments = new List<string>();

        for ( var i = metadata.HasReceiver ? 1 : 0; i < args.Length; i++ )
        {
            arguments.Add( args[i]?.ToString() ?? "null" );
        }

        return string.Join( ", ", arguments );
    }
}
