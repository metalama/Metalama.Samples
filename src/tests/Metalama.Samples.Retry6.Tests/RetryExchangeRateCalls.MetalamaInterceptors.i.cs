using Metalama.Samples.Retry.ExchangeRates;
internal static class MetalamaInterceptors
{
  internal static async Task<decimal> ExchangeRateClient_GetRateAsync_Interceptor(ExchangeRateClient receiver, string currency, CancellationToken cancellationToken)
  {
    for (var i = 0;; i++)
    {
      try
      {
        return await receiver.GetRateAsync(currency, cancellationToken);
      }
      catch (Exception e)when (i < 3)
      {
        var delay = 100 * Math.Pow(2, i + 1);
        Console.WriteLine(e.Message + $" Waiting {delay} ms.");
        await Task.Delay((int)delay, cancellationToken);
      }
    }
  }
  internal static decimal ExchangeRateClient_GetRate_Interceptor(ExchangeRateClient receiver, string currency)
  {
    for (var i = 0;; i++)
    {
      try
      {
        return receiver.GetRate(currency);
      }
      catch (Exception e)when (i < 3)
      {
        var delay = 100 * Math.Pow(2, i + 1);
        Console.WriteLine(e.Message + $" Waiting {delay} ms.");
        Thread.Sleep((int)delay);
      }
    }
  }
}