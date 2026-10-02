namespace Metalama.Samples.Comparison4;

public class VersionedEntity : Entity
{
    [EqualityMember]
    public int Version { get; init; }
}