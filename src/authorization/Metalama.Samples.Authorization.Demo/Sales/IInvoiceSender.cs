namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// Delivers an invoice to its customer, for instance by email. The application registers the implementation
/// in the dependency injection container.
/// </summary>
public interface IInvoiceSender
{
    Task SendAsync( Invoice invoice, CancellationToken cancellationToken );
}
