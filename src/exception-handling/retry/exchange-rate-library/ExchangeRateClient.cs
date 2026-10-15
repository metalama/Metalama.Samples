namespace Metalama.Samples.Retry.ExchangeRates;

// Represents the client of a remote exchange rate service, in a library that you do not own.
// To simulate transient failures, each method fails the first two times it is called.
public class ExchangeRateClient
{
    private readonly Dictionary<string, int> _callCounts = new();
    private int _orderNumber;

    // Reads a value from the service. Calling it twice has the same effect as calling it once.
    public decimal GetRate( string currency )
    {
        this.SimulateTransientFailure( nameof(this.GetRate) );

        return GetRateCore( currency );
    }

    // Reads a value from the service. Calling it twice has the same effect as calling it once.
    public async Task<decimal> GetRateAsync( string currency, CancellationToken cancellationToken = default )
    {
        await Task.Delay( 10, cancellationToken );

        this.SimulateTransientFailure( nameof(this.GetRateAsync) );

        return GetRateCore( currency );
    }

    // Places an order with the service. Each call places a new order.
    public async Task<string> BuyCurrencyAsync( string currency, decimal amount, CancellationToken cancellationToken = default )
    {
        await Task.Delay( 10, cancellationToken );

        this._orderNumber++;

        return $"Order {this._orderNumber}: {amount} {currency}";
    }

    private static decimal GetRateCore( string currency )
        => currency switch
        {
            "EUR" => 0.92m,
            "GBP" => 0.79m,
            _ => throw new ArgumentOutOfRangeException( nameof(currency), $"The currency '{currency}' is not supported." )
        };

    private void SimulateTransientFailure( string method )
    {
        this._callCounts.TryGetValue( method, out var count );
        this._callCounts[method] = count + 1;

        if ( count < 2 )
        {
            throw new TimeoutException( "The exchange rate service did not respond in time." );
        }
    }
}
