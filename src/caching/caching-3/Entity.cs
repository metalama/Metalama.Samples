public class Entity
{
    [CacheKeyMember]
    public EntityKey Key { get; }

    public Entity( EntityKey key )
    {
        this.Key = key;
    }
}