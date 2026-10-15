#if TEST_OPTIONS
// @Include(../../universal-interceptor/universal-interceptor-3/InterceptionFabric.cs)
// @Include(../../universal-interceptor/universal-interceptor-3/CallLog.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor3.Tests.AsyncLocalInterceptor;

// CallLog.Current is an AsyncLocal<IUniversalInterceptor>, so the interceptors read CallLog.Current.Value.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public double GetTemperature( string city ) => this._client.GetTemperature( city );

    public async Task<int> GetForecastDaysAsync( string city ) => (await this._client.GetForecastAsync( city, 3 )).Days;
}
