public class EntityKey
{
    [CacheKeyMember]
    public string Type { get; }

    [CacheKeyMember]
    public long Id { get; }

    public EntityKey( string type, long id )
    {
        this.Type = type;
        this.Id = id;
    }
}