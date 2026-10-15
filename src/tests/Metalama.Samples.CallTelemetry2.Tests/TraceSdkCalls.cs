#if TEST_OPTIONS
// @Include(../../call-telemetry/call-telemetry-2/TelemetryFabric.cs)
// @Include(../../call-telemetry/call-telemetry-2/PaymentsTelemetry.cs)
#endif

using Metalama.Samples.CallTelemetry.Payments;

namespace Metalama.Samples.CallTelemetry2.Tests.TraceSdkCalls;

public class CheckoutService
{
    private readonly PaymentClient _payments = new();

    public async Task<string> CheckoutAsync( string cardNumber, decimal amount, CancellationToken cancellationToken = default )
    {
        var fee = this._payments.GetFee( amount );
        var authorizationId = await this._payments.AuthorizeAsync( cardNumber, amount + fee, cancellationToken );
        await this._payments.CaptureAsync( authorizationId, cancellationToken );

        return authorizationId;
    }
}

public class PriceList
{
    private readonly PaymentClient _payments = new();

    public decimal GetPriceWithFee( decimal price ) => price + this._payments.GetFee( price );
}
