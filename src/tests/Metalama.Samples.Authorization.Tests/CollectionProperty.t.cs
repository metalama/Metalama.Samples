// Warning AUTH003 on `Approvers`: `'Document.Approvers' returns the collection type 'List<string>', which the getter cannot secure: a caller who can read it may change its content without the write permission. Declare it as IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T> or IEnumerable<T>, initialized with a SecurableList<T>, or as an immutable type.`
// Warning AUTH003 on `Sections`: `'Document.Sections' returns the collection type 'string[]', which the getter cannot secure: a caller who can read it may change its content without the write permission. Declare it as IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T> or IEnumerable<T>, initialized with a SecurableList<T>, or as an immutable type.`
// Warning AUTH003 on `Metadata`: `'Document.Metadata' returns the collection type 'Dictionary<string, string>', which the getter cannot secure: a caller who can read it may change its content without the write permission. Declare it as IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T> or IEnumerable<T>, initialized with a SecurableList<T>, or as an immutable type.`
using System.Collections.Immutable;
namespace Metalama.Samples.Authorization.Tests.CollectionProperty;
public class Document : ISecurable
{
  private readonly SecurableList<string> _authors1 = [];
  private SecurableList<string> _authors
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document._authors", AccessKind.Read);
      return SecurableCollection.SetOwner(_authors1, this, "Write", "Document._authors", "_authors");
    }
    init
    {
      _authors1 = value;
    }
  }
  private readonly IList<string> _tags = new SecurableList<string>();
  // The getter sets the owner of the securable list on the first access, so that changing the list requires Write.
  public IList<string> Tags
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Tags", AccessKind.Read);
      return SecurableCollection.SetOwner(_tags, this, "Write", "Document.Tags", "Tags");
    }
    private init
    {
      _tags = value;
    }
  }
  private readonly ICollection<int> _pages = new SecurableList<int>();
  public ICollection<int> Pages
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Pages", AccessKind.Read);
      return SecurableCollection.SetOwner(_pages, this, "Write", "Document.Pages", "Pages");
    }
    private init
    {
      _pages = value;
    }
  }
  private readonly SecurableList<string> _reviewers = [];
  // A property can also be declared with the securable type.
  public SecurableList<string> Reviewers
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Reviewers", AccessKind.Read);
      return SecurableCollection.SetOwner(_reviewers, this, "Write", "Document.Reviewers", "Reviewers");
    }
    private init
    {
      _reviewers = value;
    }
  }
  // A read-only type is secured too: casting it back to IList<T> does not bypass the check.
  public IReadOnlyList<string> Authors
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Authors", AccessKind.Read);
      return SecurableCollection.SetOwner(Authors_Source, this, "Write", "Document.Authors", "Authors");
    }
  }
  private IReadOnlyList<string> Authors_Source => this._authors;
  // A sequence that is not a collection, and an immutable type, are returned as they are.
  public IEnumerable<string> UpperTags
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.UpperTags", AccessKind.Read);
      return SecurableCollection.SetOwner(UpperTags_Source, this, "Write", "Document.UpperTags", "UpperTags");
    }
  }
  private IEnumerable<string> UpperTags_Source => this.Tags.Select(t => t.ToUpperInvariant());
  private ImmutableArray<string> _keywords = [];
  public ImmutableArray<string> Keywords
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Keywords", AccessKind.Read);
      return _keywords;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.Keywords", AccessKind.Write);
      _keywords = value;
    }
  }
  private readonly List<string> _approvers = [];
  // Types that the getter cannot secure: warning AUTH003.
  public List<string> Approvers
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Approvers", AccessKind.Read);
      return _approvers;
    }
    private init
    {
      _approvers = value;
    }
  }
  private readonly string[] _sections = [];
  public string[] Sections
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Sections", AccessKind.Read);
      return _sections;
    }
    private init
    {
      _sections = value;
    }
  }
  private readonly Dictionary<string, string> _metadata = [];
  public Dictionary<string, string> Metadata
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Metadata", AccessKind.Read);
      return _metadata;
    }
    private init
    {
      _metadata = value;
    }
  }
}