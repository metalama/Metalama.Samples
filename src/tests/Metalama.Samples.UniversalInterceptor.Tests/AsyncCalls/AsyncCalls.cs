#if TEST_OPTIONS
// @Include(../../../universal-interceptor/weather-app/InterceptionFabric.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor.Tests.AsyncCalls;

// The methods that return a Task, a ValueTask, a Task<T> and a ValueTask<T> get async interceptors, which await
// InvokeAsync.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public async Task<double> GetForecastDifferenceAsync( string city )
    {
        await this._client.RefreshAsync();
        await this._client.ClearCacheAsync();
        var forecast = await this._client.GetForecastAsync( city, 3 );

        return forecast.AverageHigh - await this._client.GetTemperatureAsync( city );
    }
}
