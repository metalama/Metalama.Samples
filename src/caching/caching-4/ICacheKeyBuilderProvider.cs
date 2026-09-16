public interface ICacheKeyBuilderProvider
{
    ICacheKeyBuilder<TValue> GetCacheKeyBuilder<TValue, TBuilder>( in TValue value )
        where TBuilder : ICacheKeyBuilder<TValue>, new();
}