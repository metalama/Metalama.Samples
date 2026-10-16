using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
using Metalama.Framework.RunTime;
using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_Namespace
{
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  public class Fabric : ProjectFabric
  {
    public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  public abstract class Entity : ISecurable, IInitializable
  {
    private string? _name;
    public string? Name
    {
      get
      {
        AccessGuard.Demand(this, "Read", "Entity.Name", AccessKind.Read);
        return _name;
      }
      set
      {
        AccessGuard.Demand(this, "Edit", "Entity.Name", AccessKind.Write);
        _name = value;
      }
    }
    protected Entity([AspectGenerated] InitializationContext context)
    {
      SecurityContext.BeginInitialization(this);
      if (!context.IsHandled(InitializationSlot.OnConstructed))
      {
        this.OnConstructed(context);
      }
    }
    protected Entity() : this(context: default)
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
  namespace Finance
  {
    // The namespace fabric overrides the project for this namespace only. It also applies to the members
    // of a class whose base class is declared in another namespace.
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
    public class FinanceFabric : NamespaceFabric
    {
      public override void AmendNamespace(INamespaceAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
    }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
    public class Budget : Entity
    {
      private decimal _amount;
      public decimal Amount
      {
        get
        {
          AccessGuard.Demand(this, "ReadSensitive", "Budget.Amount", AccessKind.Read);
          return _amount;
        }
        set
        {
          AccessGuard.Demand(this, "Edit", "Budget.Amount", AccessKind.Write);
          _amount = value;
        }
      }
      public Budget([AspectGenerated] InitializationContext context) : base(context.Descend(InitializationSlot.OnConstructed))
      {
        SecurityContext.BeginInitialization(this);
        if (!context.IsHandled(InitializationSlot.OnConstructed))
        {
          this.OnConstructed(context);
        }
      }
      public Budget() : this(context: default)
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
  }
}