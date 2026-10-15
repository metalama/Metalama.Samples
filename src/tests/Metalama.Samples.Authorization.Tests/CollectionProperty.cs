using System.Collections.Immutable;

namespace Metalama.Samples.Authorization.Tests.CollectionProperty;

public class Document : ISecurable
{
    private readonly SecurableList<string> _authors = [];

    // The getter sets the owner of the securable list on the first access, so that changing the list requires Write.
    public IList<string> Tags { get; } = new SecurableList<string>();

    public ICollection<int> Pages { get; } = new SecurableList<int>();

    // A property can also be declared with the securable type.
    public SecurableList<string> Reviewers { get; } = [];

    // A read-only type is secured too: casting it back to IList<T> does not bypass the check.
    public IReadOnlyList<string> Authors => this._authors;

    // A sequence that is not a collection, and an immutable type, are returned as they are.
    public IEnumerable<string> UpperTags => this.Tags.Select( t => t.ToUpperInvariant() );

    public ImmutableArray<string> Keywords { get; set; } = [];

    // Types that the getter cannot secure: warning AUTH003.
    public List<string> Approvers { get; } = [];

    public string[] Sections { get; } = [];

    public Dictionary<string, string> Metadata { get; } = [];
}
