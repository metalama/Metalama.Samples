namespace Metalama.Samples.Authorization.Tests.GrantsPermissions_Interface;

public class Invoice : ISecurable
{
    [RequiresPermission( "Read" )]
    public decimal Total { get; }
}

// The aspects on the parameter of an interface method are inherited by the parameter of every implementation.
public interface IInvoiceSender
{
    Task SendAsync( [RequiresPermission( "Send" )] [GrantsPermissions( "Read" )] Invoice invoice );
}

public class EmailInvoiceSender : IInvoiceSender
{
    public Task SendAsync( Invoice invoice )
    {
        _ = invoice.Total;

        return Task.CompletedTask;
    }
}
