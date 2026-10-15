using System.Collections.Immutable;

namespace Metalama.Samples.Authorization.Tests.CollectionProperty;

[ApplyDefaultPermissions]
public class Document
{
    private readonly List<string> _authors = [];

    // IList<T> and ICollection<T>: the getter returns a guarded view, so changing the content requires Write.
    public IList<string> Tags { get; } = new List<string>();

    public ICollection<int> Pages => this._pages;

    private readonly List<int> _pages = [];

    // Read-only types are returned as they are. The class must not return a mutable object behind them.
    public IReadOnlyList<string> Authors => this._authors.AsReadOnly();

    public ImmutableArray<string> Keywords { get; set; } = [];

    // Mutable types that the view cannot replace: warning AUTH003.
    public List<string> Reviewers { get; } = [];

    public string[] Sections { get; } = [];

    public Dictionary<string, string> Metadata { get; } = [];
}
