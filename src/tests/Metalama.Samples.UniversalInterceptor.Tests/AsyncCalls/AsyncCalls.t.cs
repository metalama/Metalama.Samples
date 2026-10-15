using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor.Tests.AsyncCalls;
// The methods that return a Task, a ValueTask, a Task<T> and a ValueTask<T> get async interceptors, which await
// InvokeAsync.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public async Task<double> GetForecastDifferenceAsync(string city)
  {
    await MetalamaInterceptors.WeatherClient_RefreshAsync_Interceptor(this._client, cancellationToken: default(CancellationToken));
    await MetalamaInterceptors.WeatherClient_ClearCacheAsync_Interceptor(this._client, cancellationToken: default(CancellationToken));
    var forecast = await MetalamaInterceptors.WeatherClient_GetForecastAsync_Interceptor(this._client, city: city, days: 3, cancellationToken: default(CancellationToken));
    return forecast.AverageHigh - await MetalamaInterceptors.WeatherClient_GetTemperatureAsync_Interceptor(this._client, city: city, cancellationToken: default(CancellationToken));
  }
}