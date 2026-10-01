public class BlobId
{
    public BlobId( string container, byte[] hash )
    {
        this.Container = container;
        this.Hash = hash;
    }

    [CacheKeyMember]
    public string Container { get; }

    [CacheKeyMember]
    public byte[] Hash { get; }
}