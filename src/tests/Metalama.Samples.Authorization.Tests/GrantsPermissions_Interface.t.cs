using Metalama.Framework.RunTime.Initialization;
namespace Metalama.Samples.Authorization.Tests.GrantsPermissions_Interface;
public class Invoice : ISecurable, IInitializable
{
  private readonly decimal _total;
  [RequiresPermission("Read")]
  public decimal Total
  {
    get
    {
      AccessGuard.Demand(this, "Read", "Invoice.Total", AccessKind.Read);
      return _total;
    }
    private init
    {
      _total = value;
    }
  }
  public Invoice()
  {
    SecurityContext.BeginInitialization(this);
  }
  public virtual void Initialize(InitializationContext context = default)
  {
    SecurityContext.EndInitialization(this);
  }
}
// The aspects on the parameter of an interface method are inherited by the parameter of every implementation.
public interface IInvoiceSender
{
  Task SendAsync([RequiresPermission("Send")][GrantsPermissions("Read")] Invoice invoice);
}
public class EmailInvoiceSender : IInvoiceSender
{
  public Task SendAsync(Invoice invoice)
  {
    ISecurable? argument = invoice;
    if (argument != null)
    {
      AccessGuard.Demand(argument, "Send", "EmailInvoiceSender.SendAsync(invoice)", AccessKind.Execute);
    }
    using (SecurityContext.Grant(invoice, new string[] { "Read" }))
    {
      _ = invoice.Total;
      return Task.CompletedTask;
    }
  }
}