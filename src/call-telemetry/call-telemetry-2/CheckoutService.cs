using Metalama.Samples.CallTelemetry.Payments;

namespace Metalama.Samples.CallTelemetry2;

public class CheckoutService
{
    private readonly PaymentClient _payments = new();

    public async Task<string> CheckoutAsync(
        string cardNumber,
        decimal amount,
        CancellationToken cancellationToken = default )
    {
        var fee = this._payments.GetFee( amount );
        var authorizationId = await this._payments.AuthorizeAsync( cardNumber, amount + fee, cancellationToken );
        await this._payments.CaptureAsync( authorizationId, cancellationToken );

        return authorizationId;
    }
}
