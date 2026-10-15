using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry2;

// Holds the ActivitySource of the calls to the payment SDK. The fabric generates the interceptors in this class.
internal static class PaymentsTelemetry
{
    public const string Name = "Metalama.Samples.CallTelemetry.Payments";

    public static readonly ActivitySource ActivitySource = new( Name );
}
