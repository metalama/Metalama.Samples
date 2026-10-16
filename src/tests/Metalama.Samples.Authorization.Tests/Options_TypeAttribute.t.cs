using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.Options_TypeAttribute;
[AuthorizationOptions(DefaultWritePermission = "Approve")]
public class Customer : ISecurable, IInitializable
{
  private decimal _creditLimit;
  // Write requires Approve, from the type-level option.
  public decimal CreditLimit
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Customer.CreditLimit", AccessKind.Read);
      return _creditLimit;
    }
    set
    {
      AccessGuard.Demand(this, "Approve", "Customer.CreditLimit", AccessKind.Write);
      _creditLimit = value;
    }
  }
  private string? _email;
  // The member-level option wins over the type-level option.
  [AuthorizationOptions(DefaultWritePermission = "Write")]
  public string? Email
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Customer.Email", AccessKind.Read);
      return _email;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Customer.Email", AccessKind.Write);
      _email = value;
    }
  }
  private string? _taxId;
  // An explicit attribute wins over all options.
  [RequiresPermission("ReadSensitive", "Write")]
  public string? TaxId
  {
    get
    {
      AccessGuard.Demand(this, "ReadSensitive", "Customer.TaxId", AccessKind.Read);
      return _taxId;
    }
    set
    {
      AccessGuard.Demand(this, "Write", "Customer.TaxId", AccessKind.Write);
      _taxId = value;
    }
  }
  public Customer()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}