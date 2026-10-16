namespace Metalama.Samples.Authorization.Demo.Sales;

/// <summary>
/// Delivers an invoice to its customer, for instance by email. The application registers the implementation
/// in the dependency injection container.
/// </summary>
public interface IInvoiceSender
{
    /// <summary>
    /// Sends <paramref name="invoice"/>. The current user must have the <see cref="DemoPermissions.Send"/> permission
    /// on the invoice, so that no code can send an invoice for a user who may not send it. Sending an invoice
    /// includes reading it, so the implementation is granted <see cref="PermissionNames.Read"/> on the invoice while
    /// it runs. The aspects are inherited, so every implementation gets the check and the grant.
    /// </summary>
    Task SendAsync(
        [RequiresPermission( DemoPermissions.Send )] [GrantsPermissions( PermissionNames.Read )] Invoice invoice,
        CancellationToken cancellationToken );
}
