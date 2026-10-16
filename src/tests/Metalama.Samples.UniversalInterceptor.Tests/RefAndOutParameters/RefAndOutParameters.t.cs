using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor.Tests.RefAndOutParameters;
// The ref and out arguments are copied into the tuple, and written back from the tuple after the call.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public int? GetHumidity(string city) => MetalamaInterceptors.WeatherClient_TryGetHumidity_Interceptor(this._client, city: city, humidity: out var humidity) ? humidity : null;
  public string[] GetAlerts(ref int cursor) => MetalamaInterceptors.WeatherClient_GetAlerts_Interceptor(this._client, cursor: ref cursor, pageSize: 2);
}