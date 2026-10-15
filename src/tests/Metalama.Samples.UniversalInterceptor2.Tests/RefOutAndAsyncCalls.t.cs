using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor2.Tests.RefOutAndAsyncCalls;
// The ref and out arguments are written back from the tuple after the call. The methods that return a Task, a Task<T>,
// a ValueTask and a ValueTask<T> get async interceptors. The call on the RequestQuota struct is not intercepted.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public int? GetHumidity(string city) => MetalamaInterceptors.WeatherClient_TryGetHumidity_Interceptor(this._client, city: city, humidity: out var humidity) ? humidity : null;
  public string[] GetFirstAlerts(ref int cursor) => MetalamaInterceptors.WeatherClient_GetAlerts_Interceptor(this._client, cursor: ref cursor, pageSize: 2);
  public async Task<double> GetForecastAsync(string city)
  {
    await MetalamaInterceptors.WeatherClient_RefreshAsync_Interceptor(this._client, cancellationToken: default(CancellationToken));
    await MetalamaInterceptors.WeatherClient_ClearCacheAsync_Interceptor(this._client, cancellationToken: default(CancellationToken));
    var forecast = await MetalamaInterceptors.WeatherClient_GetForecastAsync_Interceptor(this._client, city: city, days: 3, cancellationToken: default(CancellationToken));
    return forecast.AverageHigh - await MetalamaInterceptors.WeatherClient_GetTemperatureAsync_Interceptor(this._client, city: city, cancellationToken: default(CancellationToken));
  }
  public bool TrySendRequest()
  {
    var quota = MetalamaInterceptors.WeatherClient_GetQuota_Interceptor(this._client);
    return quota.TryConsume();
  }
}