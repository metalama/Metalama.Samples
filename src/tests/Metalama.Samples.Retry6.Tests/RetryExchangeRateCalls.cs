#if TEST_OPTIONS
// @Include(../../exception-handling/retry/retry-6/RetryFabric.cs)
#endif

using Metalama.Samples.Retry.ExchangeRates;

namespace Metalama.Samples.Retry6.Tests.RetryExchangeRateCalls;

public class PriceService
{
    private readonly ExchangeRateClient _client = new();

    // GetRate is idempotent, so the call is retried.
    public decimal ConvertPrice( decimal price, string currency ) => price * this._client.GetRate( currency );

    // GetRateAsync is idempotent, so the call is retried, and the interceptor is async.
    public async Task<decimal> ConvertPriceAsync( decimal price, string currency, CancellationToken cancellationToken )
        => price * await this._client.GetRateAsync( currency, cancellationToken );

    // BuyCurrencyAsync is not idempotent, so the call is not intercepted.
    public Task<string> BuyCurrencyAsync( string currency, decimal amount, CancellationToken cancellationToken )
        => this._client.BuyCurrencyAsync( currency, amount, cancellationToken );
}
