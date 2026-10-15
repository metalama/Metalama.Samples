namespace Metalama.Samples.Authorization.Tests.DefaultPermissionsInherited;
public abstract class Entity : ISecurable
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
      AccessGuard.Demand(this, "Write", "Entity.Name", AccessKind.Write);
      _name = value;
    }
  }
}
// The aspect is [Inheritable], so the derived class gets it without an attribute.
public class Invoice : Entity
{
  private decimal _amount;
  public decimal Amount
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Invoice.Amount", AccessKind.Read);
      return _amount;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Invoice.Amount", AccessKind.Write);
      _amount = value;
    }
  }
}