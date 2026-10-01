namespace Metalama.Samples.Comparison4;

public partial class Entity
{
    [StringEqualityMember( StringComparison.InvariantCultureIgnoreCase )]
    public required string EntityType { get; init; }

    [EqualityMember]
    public int Id { get; init; }

    public Guid ObjectId { get; } = Guid.NewGuid();
}