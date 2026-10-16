using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor;

// Calls WeatherClient. The source code calls the methods directly, and the fabric redirects every call to the
// interceptor.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    // [<snippet SynchronousCalls>]
    // Synchronous methods, with and without a return value.
    public string Describe( string city, TemperatureUnit unit = TemperatureUnit.Celsius )
    {
        try
        {
            this._client.SetUnit( unit );
            var temperature = this._client.GetTemperature( city );

            return $"{city}: {temperature} {unit}";
        }
        catch ( ArgumentException )
        {
            return $"{city}: unknown city";
        }
    }

    // [<endsnippet SynchronousCalls>]

    // [<snippet AsyncCalls>]
    // Methods that return a Task, a ValueTask, a Task<T> and a ValueTask<T>.
    public async Task<string> DescribeForecastAsync( string city, int days )
    {
        await this._client.RefreshAsync();
        await this._client.ClearCacheAsync();
        var forecast = await this._client.GetForecastAsync( city, days );
        var current = await this._client.GetTemperatureAsync( city );

        return $"{city}: {current} now, {forecast.AverageHigh} on average in the next {forecast.Days} days";
    }

    // [<endsnippet AsyncCalls>]

    // [<snippet RefOutCalls>]
    // A method with an out parameter.
    public string DescribeHumidity( string city )
        => this._client.TryGetHumidity( city, out var humidity )
            ? $"{city}: humidity {humidity} %"
            : $"{city}: humidity unknown";

    // A method with a ref parameter: GetAlerts moves the cursor after the returned alerts.
    public List<string> GetAllAlerts()
    {
        var alerts = new List<string>();
        var cursor = 0;
        string[] page;

        while ( (page = this._client.GetAlerts( ref cursor, 2 )).Length > 0 )
        {
            alerts.AddRange( page );
        }

        return alerts;
    }

    // [<endsnippet RefOutCalls>]

    // [<snippet StructReceiver>]
    // A call on a struct receiver: TryConsume changes the quota, so the call is not intercepted.
    public int CountAvailableRequests()
    {
        var quota = this._client.GetQuota();
        var count = 0;

        while ( quota.TryConsume() )
        {
            count++;
        }

        return count;
    }

    // [<endsnippet StructReceiver>]
}
