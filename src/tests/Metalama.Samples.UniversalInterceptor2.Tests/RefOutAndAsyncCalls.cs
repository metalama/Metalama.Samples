#if TEST_OPTIONS
// @Include(../../universal-interceptor/universal-interceptor-2/InterceptionFabric.cs)
// @Include(../../universal-interceptor/universal-interceptor-2/CallLog.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor2.Tests.RefOutAndAsyncCalls;

// The ref and out arguments are written back from the tuple after the call. The methods that return a Task, a Task<T>,
// a ValueTask and a ValueTask<T> get async interceptors. The call on the RequestQuota struct is not intercepted.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public int? GetHumidity( string city ) => this._client.TryGetHumidity( city, out var humidity ) ? humidity : null;

    public string[] GetFirstAlerts( ref int cursor ) => this._client.GetAlerts( ref cursor, 2 );

    public async Task<double> GetForecastAsync( string city )
    {
        await this._client.RefreshAsync();
        await this._client.ClearCacheAsync();
        var forecast = await this._client.GetForecastAsync( city, 3 );

        return forecast.AverageHigh - await this._client.GetTemperatureAsync( city );
    }

    public bool TrySendRequest()
    {
        var quota = this._client.GetQuota();

        return quota.TryConsume();
    }
}
