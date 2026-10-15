namespace Metalama.Samples.CallTelemetry.Payments;

// A client of a payment service. In a real application, this class would be part of a third-party SDK.
public class PaymentClient
{
    public decimal GetFee( decimal amount ) => Math.Round( (amount * 0.029m) + 0.30m, 2 );

    public async Task<string> AuthorizeAsync(
        string cardNumber,
        decimal amount,
        CancellationToken cancellationToken = default )
    {
        // Simulates the latency of the service.
        await Task.Delay( 20, cancellationToken );

        if ( cardNumber.EndsWith( "0000", StringComparison.Ordinal ) )
        {
            throw new InvalidOperationException( "The card was declined." );
        }

        return $"AUTH-{cardNumber[^4..]}-{amount:F2}";
    }

    public async Task CaptureAsync( string authorizationId, CancellationToken cancellationToken = default )
    {
        // Simulates the latency of the service.
        await Task.Delay( 10, cancellationToken );
    }
}
