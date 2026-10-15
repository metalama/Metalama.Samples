using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
namespace Metalama.Samples.Authorization.Tests.Options_Namespace
{
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  public class Fabric : ProjectFabric
  {
    public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  [SecurableType]
  public abstract class Entity
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
    }
  }
}