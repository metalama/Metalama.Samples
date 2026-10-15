using Metalama.Samples.CallTelemetry2;
using System.Diagnostics;

// Writes each span to the console. In a real application, an OpenTelemetry exporter collects the spans instead.
using var listener = new ActivityListener
{
    ShouldListenTo = source => source.Name == PaymentsTelemetry.Name,
    Sample = ( ref ActivityCreationOptions<ActivityContext> _ ) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = activity => Console.WriteLine(
        $"[span] {activity.DisplayName}, caller: {activity.GetTagItem( "code.caller" )}, "
        + $"status: {activity.Status}, duration: {activity.Duration.TotalMilliseconds:F1} ms" )
};

ActivitySource.AddActivityListener( listener );

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
