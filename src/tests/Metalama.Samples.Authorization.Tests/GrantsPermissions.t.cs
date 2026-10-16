using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.GrantsPermissions;
public class Folder : ISecurable, IInitializable
{
  private string? _owner;
  [RequiresPermission("ReadSensitive")]
  public string? Owner
  {
    get
    {
      AccessGuard.Demand(this, "ReadSensitive", "Folder.Owner", AccessKind.Read);
      return _owner;
    }
    set
    {
      AccessGuard.Demand(this, "ReadSensitive", "Folder.Owner", AccessKind.Write);
      _owner = value;
    }
  }
  public Folder([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Folder() : this(context: default)
  {
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
  protected virtual void OnConstructed(InitializationContext context = default)
  {
    SecurityContext.EndConstruction(this);
  }
}
public class Report : ISecurable, IInitializable
{
  private string? _body;
  [RequiresPermission("Read", "Write")]
  public string? Body
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Report.Body", AccessKind.Read);
      return _body;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Report.Body", AccessKind.Write);
      _body = value;
    }
  }
  // The check of the member runs first, outside of the grant, so the grant cannot satisfy it.
  [RequiresPermission("Summarize")]
  [GrantsPermissions("Read")]
  public int BodyLength
  {
    get
    {
      AccessGuard.Demand(this, "Summarize", "Report.BodyLength", AccessKind.Read);
      using (SecurityContext.Grant(this, new string[] { "Read" }))
      {
        return this.Body?.Length ?? 0;
      }
    }
  }
  // The check of the parameter also runs before the grant.
  [RequiresPermission("Publish")]
  [GrantsPermissions("Read", "Write")]
  public void Publish([RequiresPermission("Read")] Folder folder)
  {
    ISecurable? argument = folder;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Read", "Report.Publish(folder)", AccessKind.Execute);
    }
    AccessGuard.Demand(this, "Publish", "Report.Publish", AccessKind.Execute);
    using (SecurityContext.Grant(this, new string[] { "Read", "Write" }))
    {
      this.Body += " (published)";
    }
  }
  // On a parameter, the grant is on the argument, after the check of the argument.
  [RequiresPermission("Publish")]
  public bool IsOwnedBySomeone([RequiresPermission("Read")][GrantsPermissions("ReadSensitive")] Folder folder)
  {
    ISecurable? argument = folder;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Read", "Report.IsOwnedBySomeone(folder)", AccessKind.Execute);
    }
    AccessGuard.Demand(this, "Publish", "Report.IsOwnedBySomeone", AccessKind.Execute);
    using (SecurityContext.Grant(folder, new string[] { "ReadSensitive" }))
    {
      return folder.Owner != null;
    }
  }
  // The aspect is inherited: the overrides grant the same permissions.
  [RequiresPermission("Summarize")]
  [GrantsPermissions("Read")]
  public virtual int Score
  {
    get
    {
      AccessGuard.Demand(this, "Summarize", "Report.Score", AccessKind.Read);
      using (SecurityContext.Grant(this, new string[] { "Read" }))
      {
        return this.Body?.Length ?? 0;
      }
    }
  }
  // In an async method, the grant covers the whole execution, including the continuations.
  [RequiresPermission("Publish")]
  [GrantsPermissions("Read")]
  public async Task<int> CountWordsAsync(CancellationToken cancellationToken)
  {
    AccessGuard.Demand(this, "Publish", "Report.CountWordsAsync", AccessKind.Execute);
    using (SecurityContext.Grant(this, new string[] { "Read" }))
    {
      await Task.Delay(1, cancellationToken);
      return this.Body?.Split(' ').Length ?? 0;
    }
  }
  public Report([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Report() : this(context: default)
  {
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
  protected virtual void OnConstructed(InitializationContext context = default)
  {
    SecurityContext.EndConstruction(this);
  }
}
public class WeightedReport : Report
{
  public override int Score
  {
    get
    {
      AccessGuard.Demand(this, "Summarize", "WeightedReport.Score", AccessKind.Read);
      using (SecurityContext.Grant(this, new string[] { "Read" }))
      {
        return 2 * (this.Body?.Length ?? 0);
      }
    }
  }
  public WeightedReport([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public WeightedReport() : this(context: default)
  {
  }
  public override void Initialize(InitializationContext context = default)
  {
    base.Initialize(context);
    SecurityContext.EndInitialization(this);
  }
  protected override void OnConstructed(InitializationContext context = default)
  {
    base.OnConstructed(context);
    SecurityContext.EndConstruction(this);
  }
}
// The check can also come from the default securing of SecurableType.
public class Summary : ISecurable, IInitializable
{
  private string? _text;
  public string? Text
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Summary.Text", AccessKind.Read);
      return _text;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Summary.Text", AccessKind.Write);
      _text = value;
    }
  }
  [GrantsPermissions("Read")]
  public int TextLength
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Summary.TextLength", AccessKind.Read);
      using (SecurityContext.Grant(this, new string[] { "Read" }))
      {
        return this.Text?.Length ?? 0;
      }
    }
  }
  public Summary([AspectGenerated] InitializationContext context)
  {
    SecurityContext.BeginInitialization(this);
    if (!context.IsHandled(InitializationSlot.OnConstructed))
    {
      this.OnConstructed(context);
    }
  }
  public Summary() : this(context: default)
  {
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
  protected virtual void OnConstructed(InitializationContext context = default)
  {
    SecurityContext.EndConstruction(this);
  }
}