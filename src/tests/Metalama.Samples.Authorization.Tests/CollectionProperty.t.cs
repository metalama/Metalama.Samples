// Warning AUTH003 on `Reviewers`: `'Document.Reviewers' exposes the mutable collection type 'List<string>': any caller who can read it can change its content without the write permission. Declare it as IList<T> or ICollection<T>, which the getter wraps in a guarded view, or as a read-only type.`
// Warning AUTH003 on `Sections`: `'Document.Sections' exposes the mutable collection type 'string[]': any caller who can read it can change its content without the write permission. Declare it as IList<T> or ICollection<T>, which the getter wraps in a guarded view, or as a read-only type.`
// Warning AUTH003 on `Metadata`: `'Document.Metadata' exposes the mutable collection type 'Dictionary<string, string>': any caller who can read it can change its content without the write permission. Declare it as IList<T> or ICollection<T>, which the getter wraps in a guarded view, or as a read-only type.`
using System.Collections.Immutable;
namespace Metalama.Samples.Authorization.Tests.CollectionProperty;
[ApplyDefaultPermissions]
public class Document
{
  private readonly List<string> _authors = [];
  private readonly IList<string> _tags = new List<string>();
  // IList<T> and ICollection<T>: the getter returns a guarded view, so changing the content requires Write.
  public IList<string> Tags
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Tags", AccessKind.Read);
      return GuardedCollection.Wrap(_tags, this, "Write", "Document.Tags");
    }
    private init
    {
      _tags = value;
    }
  }
  public ICollection<int> Pages
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Pages", AccessKind.Read);
      return GuardedCollection.Wrap(Pages_Source, this, "Write", "Document.Pages");
    }
  }
  private ICollection<int> Pages_Source => this._pages;
  private readonly List<int> _pages = [];
  // Read-only types are returned as they are. The class must not return a mutable object behind them.
  public IReadOnlyList<string> Authors
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Authors", AccessKind.Read);
      return this._authors.AsReadOnly();
    }
  }
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
  private readonly List<string> _reviewers = [];
  // Mutable types that the view cannot replace: warning AUTH003.
  public List<string> Reviewers
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Reviewers", AccessKind.Read);
      return _reviewers;
    }
    private init
    {
      _reviewers = value;
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