namespace Metalama.Samples.Comparison3;

public partial class Entity
{
    [EqualityMember]
    public int Id { get; init; }

    public Guid ObjectId { get; } = Guid.NewGuid();
}