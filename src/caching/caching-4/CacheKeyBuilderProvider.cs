internal class CacheKeyBuilderProvider : ICacheKeyBuilderProvider
{
    public ICacheKeyBuilder<TValue> GetCacheKeyBuilder<TValue, TBuilder>( in TValue value )
        where TBuilder : ICacheKeyBuilder<TValue>, new()
        => Instance<TBuilder>.Value;

    private static class Instance<TBuilder>
        where TBuilder : new()
    {
        public static TBuilder Value { get; } = new();
    }
}