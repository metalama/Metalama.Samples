using System.Collections.Concurrent;

internal static class ConcurrentDictionaryExtensions
{
    // https://stackoverflow.com/a/66118283/41071
    public static Dictionary<TKey, TValue> RemoveAll<TKey, TValue>( this ConcurrentDictionary<TKey, TValue> source )
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();

        foreach ( var (key, _) in source )
        {
            if ( !result.ContainsKey( key ) && source.TryRemove( key, out var value ) )
            {
                result.Add( key, value );
            }
        }

        return result;
    }
}