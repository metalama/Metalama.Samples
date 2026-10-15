#if TEST_OPTIONS
// @Include(../../call-telemetry/call-telemetry-3/TelemetryFabric.cs)
#endif

using Metalama.Samples.CallTelemetry.Payments;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Metalama.Samples.CallTelemetry3
{
    // A copy of PaymentsTelemetry.cs. It is in the test file so that the output shows the generated interceptors.
    internal static class PaymentsTelemetry
    {
        public const string Name = "Metalama.Samples.CallTelemetry.Payments";

        public static readonly ActivitySource ActivitySource = new( Name );

        private static readonly Meter _meter = new( Name );

        public static readonly Counter<long> Calls = _meter.CreateCounter<long>( "payments.client.calls" );

        public static readonly Histogram<double> Duration =
            _meter.CreateHistogram<double>( "payments.client.duration", unit: "ms" );
    }
}

namespace Metalama.Samples.CallTelemetry3.Tests.RecordSdkMetrics
{
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
}
