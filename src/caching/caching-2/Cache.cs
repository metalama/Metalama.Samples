using System.Collections.Concurrent;

// A trivial implementation of ICache whose main benefit is to work in the Metalama online sandbox.
// In production, use MemoryCache.

internal class Cache : ICache
{
    private readonly ConcurrentDictionary<string, object?> _impl = new();

    public bool TryGetValue( string key, out object? value )
    {
        if ( this._impl.TryGetValue( key, out value ) )
        {
            Console.WriteLine( $"Key '{key}' found in cache." );

            return true;
        }

        Console.WriteLine( $"Key '{key}' not found in cache." );

        return false;
    }

    public void TryAdd( string key, object? value )
    {
        if ( this._impl.TryAdd( key, value ) )
        {
            Console.WriteLine( $"Key '{key}' added cache." );
        }
    }
}