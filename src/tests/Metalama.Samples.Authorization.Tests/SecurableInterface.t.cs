using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.SecurableInterface;
// Implementing ISecurable is enough: the class inherits the SecurableType aspect from the interface,
// so its members are secured by default and validated.
public class Document : ISecurable, IInitializable
{
  private string? _title;
  public string? Title
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Document.Title", AccessKind.Read);
      return _title;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Document.Title", AccessKind.Write);
      _title = value;
    }
  }
  public void Publish()
  {
    AccessGuard.Demand(this, "Write", "Document.Publish", AccessKind.Execute);
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
// The aspect is also inherited through a derived interface, and by derived classes.
public interface IArchive : ISecurable;
public class Archive : IArchive, IInitializable
{
  private int _capacity;
  public int Capacity
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Archive.Capacity", AccessKind.Read);
      return _capacity;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Archive.Capacity", AccessKind.Write);
      _capacity = value;
    }
  }
  public Archive()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
public class ColdArchive : Archive
{
  private string? _location;
  public string? Location
  {
    get
    {
      AccessGuard.Demand(this, "Read", "ColdArchive.Location", AccessKind.Read);
      return _location;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "ColdArchive.Location", AccessKind.Write);
      _location = value;
    }
  }
  public ColdArchive()
  {
    SecurityContext.BeginInitialization(this);
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
}