using Metalama.Samples.CallTelemetry3;
using System.Diagnostics.Metrics;

// Writes each measurement to the console. In a real application, an OpenTelemetry exporter collects the
// measurements instead.
using var listener = new MeterListener
{
    InstrumentPublished = ( instrument, listener ) =>
    {
        if ( instrument.Meter.Name == PaymentsTelemetry.Name )
        {
            listener.EnableMeasurementEvents( instrument );
        }
    }
};

listener.SetMeasurementEventCallback<long>(
    ( instrument, value, tags, _ ) => WriteMeasurement( instrument, value, tags ) );

listener.SetMeasurementEventCallback<double>(
    ( instrument, value, tags, _ ) => WriteMeasurement( instrument, value, tags ) );

listener.Start();

Console.WriteLine( $"Price with fee: {new PriceList().GetPriceWithFee( 100 )}" );

var checkout = new CheckoutService();
Console.WriteLine( $"Authorization: {await checkout.CheckoutAsync( "4111111111111111", 100 )}" );

try
{
    await checkout.CheckoutAsync( "4000000000000000", 100 );
}
catch ( InvalidOperationException e )
{
    Console.WriteLine( $"Checkout failed: {e.Message}" );
}

static void WriteMeasurement<T>( Instrument instrument, T value, ReadOnlySpan<KeyValuePair<string, object?>> tags )
{
    var tagList = new List<string>();

    foreach ( var tag in tags )
    {
        tagList.Add( $"{tag.Key}={tag.Value}" );
    }

    Console.WriteLine(
        $"[metric] {instrument.Name} {value:0.#}{(instrument.Unit == null ? "" : " " + instrument.Unit)} "
        + $"{{{string.Join( ", ", tagList )}}}" );
}
