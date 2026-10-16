using Metalama.Samples.UniversalInterceptor.Weather;
namespace Metalama.Samples.UniversalInterceptor.Tests.SynchronousCalls;
// The calls to a method with a return value and to a void method are redirected to CallLog.Current.
public class WeatherReport
{
  private readonly WeatherClient _client = new();
  public double GetTemperature(string city, TemperatureUnit unit)
  {
    MetalamaInterceptors.WeatherClient_SetUnit_Interceptor(this._client, unit: unit);
    return MetalamaInterceptors.WeatherClient_GetTemperature_Interceptor(this._client, city: city);
  }
}