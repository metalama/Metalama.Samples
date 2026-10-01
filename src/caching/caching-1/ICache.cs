public interface ICache
{
    bool TryGetValue( string key, out object? value );

    void TryAdd( string key, object? value );
}