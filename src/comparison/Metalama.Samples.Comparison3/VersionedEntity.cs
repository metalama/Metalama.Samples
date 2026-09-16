namespace Metalama.Samples.Comparison3;

public class VersionedEntity : Entity
{
    [EqualityMember]
    public int Version { get; init; }
}