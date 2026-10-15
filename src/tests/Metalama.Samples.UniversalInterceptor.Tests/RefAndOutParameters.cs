#if TEST_OPTIONS
// @Include(../../universal-interceptor/weather-app/InterceptionFabric.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor.Tests.RefAndOutParameters;

// The ref and out arguments are copied into the tuple, and written back from the tuple after the call.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public int? GetHumidity( string city ) => this._client.TryGetHumidity( city, out var humidity ) ? humidity : null;

    public string[] GetAlerts( ref int cursor ) => this._client.GetAlerts( ref cursor, 2 );
}
