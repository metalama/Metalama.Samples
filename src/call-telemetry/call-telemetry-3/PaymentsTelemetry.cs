using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Metalama.Samples.CallTelemetry3;

// Holds the ActivitySource and the metrics of the calls to the payment SDK. The fabric generates the interceptors
// in this class.
internal static class PaymentsTelemetry
{
    public const string Name = "Metalama.Samples.CallTelemetry.Payments";

    public static readonly ActivitySource ActivitySource = new( Name );

    private static readonly Meter _meter = new( Name );

    public static readonly Counter<long> Calls = _meter.CreateCounter<long>( "payments.client.calls" );

    public static readonly Histogram<double> Duration =
        _meter.CreateHistogram<double>( "payments.client.duration", unit: "ms" );
}
