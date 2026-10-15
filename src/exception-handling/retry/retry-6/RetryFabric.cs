using Metalama.Extensions.Interceptors;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.Samples.Retry.ExchangeRates;

namespace Metalama.Samples.Retry6;

// Retries the calls to the idempotent methods of ExchangeRateClient, a class of a library that you do not own.
internal class RetryFabric : ProjectFabric
{
    /// <summary>
    /// Gets the maximum number of times that the call should be retried.
    /// </summary>
    private int Attempts => 3;

    /// <summary>
    /// Gets the delay, in ms, to wait before the first retry.
    /// The delay is doubled at every further retry.
    /// </summary>
    private double Delay => 100;

    public override void AmendProject( IProjectAmender amender )
        => amender.InterceptMethods(
            calls => calls
                .Type( typeof(ExchangeRateClient) )
                .AllMethods()

                // Retry only the methods that read data. Retrying BuyCurrencyAsync could place the order twice.
                .Where( m => m.Name.StartsWith( "Get", StringComparison.Ordinal ) )
                .RedirectToSynthesizedMethod(
                    new MethodTemplateSelector( nameof(this.Retry), nameof(this.RetryAsync) ) ) );

    // Template for non-async methods.
    [Template]
    public dynamic? Retry()
    {
        for ( var i = 0;; i++ )
        {
            try
            {
                return meta.Proceed();
            }
            catch ( Exception e ) when ( i < this.Attempts )
            {
                var delay = this.Delay * Math.Pow( 2, i + 1 );
                Console.WriteLine( e.Message + $" Waiting {delay} ms." );
                Thread.Sleep( (int) delay );
            }
        }
    }

    // Template for async methods.
    [Template]
    public async Task<dynamic?> RetryAsync()
    {
        var cancellationTokenParameter = meta.Interception.Method.ArgumentParameters
            .LastOrDefault( p => p.Type.Equals( typeof(CancellationToken) ) );

        for ( var i = 0;; i++ )
        {
            try
            {
                return await meta.ProceedAsync();
            }
            catch ( Exception e ) when ( i < this.Attempts )
            {
                var delay = this.Delay * Math.Pow( 2, i + 1 );
                Console.WriteLine( e.Message + $" Waiting {delay} ms." );

                if ( cancellationTokenParameter != null )
                {
                    await Task.Delay( (int) delay, cancellationTokenParameter.Value );
                }
                else
                {
                    await Task.Delay( (int) delay );
                }
            }
        }
    }
}
