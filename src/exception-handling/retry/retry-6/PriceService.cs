using Metalama.Samples.Retry.ExchangeRates;

namespace Metalama.Samples.Retry6;

public class PriceService
{
    private readonly ExchangeRateClient _client = new();

    public decimal ConvertPrice( decimal price, string currency ) => price * this._client.GetRate( currency );

    public async Task<decimal> ConvertPriceAsync( decimal price, string currency, CancellationToken cancellationToken )
        => price * await this._client.GetRateAsync( currency, cancellationToken );

    public Task<string> BuyCurrencyAsync( string currency, decimal amount, CancellationToken cancellationToken )
        => this._client.BuyCurrencyAsync( currency, amount, cancellationToken );
}
