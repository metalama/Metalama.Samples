#if TEST_OPTIONS
// @Include(../../universal-interceptor/weather-app/InterceptionFabric.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor.Tests.SynchronousCalls;

// The calls to a method with a return value and to a void method are redirected to CallLog.Current.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public double GetTemperature( string city, TemperatureUnit unit )
    {
        this._client.SetUnit( unit );

        return this._client.GetTemperature( city );
    }
}
