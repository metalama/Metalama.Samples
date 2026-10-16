using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.InitSetter;
public class Document : ISecurable, IInitializable
{
  private readonly string? _title;
  // An init accessor is checked: an object initializer written by the caller is a write.
  [RequiresPermission("Read", "Create")]
  public string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Title", AccessKind.Read);
      return _title;
    }
    init
    {
      AccessGuard.Demand(this, "Create", "Document.Title", AccessKind.Write);
      _title = value;
    }
  }
  public Document()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}