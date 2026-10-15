using System.Runtime.CompilerServices;

namespace Metalama.Samples.Proxy;

public sealed class LoggingInterceptor : IInterceptor
{
    private readonly TextWriter _writer;

    public LoggingInterceptor( TextWriter writer )
    {
        this._writer = writer;
    }

    public TResult Invoke<TArgs, TResult>(
        ref TArgs args,
        InterceptionMetadata metadata,
        InterceptorDelegate<TArgs, TResult> proceed )
        where TArgs : struct, ITuple
    {
        this._writer.WriteLine( $"{Format( metadata, args )} started." );

        try
        {
            var result = proceed( ref args );

            this._writer.WriteLine( $"{Format( metadata, args )} returned {FormatResult( result )}." );

            return result;
        }
        catch ( Exception e )
        {
            this._writer.WriteLine( $"{Format( metadata, args )} failed: {e.GetType().Name}." );

            throw;
        }
    }

    public async Task<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, Task<TResult>> proceed )
        where TArgs : struct, ITuple
    {
        this._writer.WriteLine( $"{Format( metadata, args )} started." );

        try
        {
            var result = await proceed( args );

            this._writer.WriteLine( $"{Format( metadata, args )} returned {FormatResult( result )}." );

            return result;
        }
        catch ( Exception e )
        {
            this._writer.WriteLine( $"{Format( metadata, args )} failed: {e.GetType().Name}." );

            throw;
        }
    }

    public async ValueTask<TResult> InvokeAsync<TArgs, TResult>(
        TArgs args,
        InterceptionMetadata metadata,
        Func<TArgs, ValueTask<TResult>> proceed )
        where TArgs : struct, ITuple
    {
        this._writer.WriteLine( $"{Format( metadata, args )} started." );

        try
        {
            var result = await proceed( args );

            this._writer.WriteLine( $"{Format( metadata, args )} returned {FormatResult( result )}." );

            return result;
        }
        catch ( Exception e )
        {
            this._writer.WriteLine( $"{Format( metadata, args )} failed: {e.GetType().Name}." );

            throw;
        }
    }

    private static string Format<TArgs>( InterceptionMetadata metadata, TArgs args )
        where TArgs : struct, ITuple
    {
        var arguments = new string[args.Length];

        for ( var i = 0; i < args.Length; i++ )
        {
            arguments[i] = args[i]?.ToString() ?? "null";
        }

        return $"{metadata.Method.Name}({string.Join( ", ", arguments )})";
    }

    // A void method is projected to a ValueTuple result, which has no meaningful text representation.
    private static string FormatResult<TResult>( TResult result )
        => typeof(TResult) == typeof(ValueTuple) ? "void" : result?.ToString() ?? "null";
}
