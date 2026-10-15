namespace Metalama.Samples.Authorization.Tests.Options_Disabled;
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
      AccessGuard.Demand(this, "Write", "Entity.Name", AccessKind.Write);
      _name = value;
    }
  }
}
// Default securing can be disabled for a type...
[AuthorizationOptions(IsDefaultSecuringEnabled = false)]
public class LookupValue : Entity
{
  public string? Code { get; set; }
}
public class Invoice : Entity
{
  // ...or for a single member.
  [AuthorizationOptions(IsDefaultSecuringEnabled = false)]
  public string? Number { get; set; }
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