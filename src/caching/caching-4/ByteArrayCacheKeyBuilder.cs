internal class ByteArrayCacheKeyBuilder : ICacheKeyBuilder<byte[]?>
{
    public string? GetCacheKey( in byte[]? value, ICacheKeyBuilderProvider provider )
    {
        if ( value == null )
        {
            return null;
        }

        return string.Join( ' ', value );
    }
}