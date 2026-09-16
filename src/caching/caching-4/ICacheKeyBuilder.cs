public interface ICacheKeyBuilder<T>
{
    string? GetCacheKey( in T value, ICacheKeyBuilderProvider provider );
}