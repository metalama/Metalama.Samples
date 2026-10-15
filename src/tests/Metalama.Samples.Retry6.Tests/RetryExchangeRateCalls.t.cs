using Metalama.Samples.Retry.ExchangeRates;
namespace Metalama.Samples.Retry6.Tests.RetryExchangeRateCalls;
public class PriceService
{
  private readonly ExchangeRateClient _client = new();
  // GetRate is idempotent, so the call is retried.
  public decimal ConvertPrice(decimal price, string currency) => price * MetalamaInterceptors.ExchangeRateClient_GetRate_Interceptor(this._client, currency: currency);
  // GetRateAsync is idempotent, so the call is retried, and the interceptor is async.
  public async Task<decimal> ConvertPriceAsync(decimal price, string currency, CancellationToken cancellationToken) => price * await MetalamaInterceptors.ExchangeRateClient_GetRateAsync_Interceptor(this._client, currency: currency, cancellationToken: cancellationToken);
  // BuyCurrencyAsync is not idempotent, so the call is not intercepted.
  public Task<string> BuyCurrencyAsync(string currency, decimal amount, CancellationToken cancellationToken) => this._client.BuyCurrencyAsync(currency, amount, cancellationToken);
}