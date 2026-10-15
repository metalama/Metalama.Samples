using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor3.Tests.AsyncLocalInterceptor;
// CallLog.Current is an AsyncLocal<IUniversalInterceptor>, so the interceptors read CallLog.Current.Value.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public double GetTemperature(string city) => MetalamaInterceptors.WeatherClient_GetTemperature_Interceptor(this._client, city: city);
  public async Task<int> GetForecastDaysAsync(string city) => (await MetalamaInterceptors.WeatherClient_GetForecastAsync_Interceptor(this._client, city: city, days: 3, cancellationToken: default(CancellationToken))).Days;
}