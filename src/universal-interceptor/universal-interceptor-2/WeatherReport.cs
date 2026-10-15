using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor2;

public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public void PrintHumidity( string city )
    {
        // An out parameter.
        if ( this._client.TryGetHumidity( city, out var humidity ) )
        {
            Console.WriteLine( $"{city}: humidity {humidity} %" );
        }
        else
        {
            Console.WriteLine( $"{city}: humidity unknown" );
        }
    }

    public void PrintAlerts()
    {
        // A ref parameter: GetAlerts moves the cursor.
        var cursor = 0;
        string[] page;

        while ( (page = this._client.GetAlerts( ref cursor, 2 )).Length > 0 )
        {
            Console.WriteLine( $"Alerts: {string.Join( " ", page )}" );
        }
    }

    public async Task PrintForecastAsync( string city )
    {
        // Methods that return a Task, a Task<T>, a ValueTask and a ValueTask<T>.
        await this._client.RefreshAsync();
        await this._client.ClearCacheAsync();
        var forecast = await this._client.GetForecastAsync( city, 3 );
        var current = await this._client.GetTemperatureAsync( city );

        Console.WriteLine( $"{city}: {current} now, {forecast.AverageHigh} on average in the next {forecast.Days} days" );
    }

    public void PrintRemainingRequests()
    {
        // A call on a struct receiver: TryConsume changes the quota, so the call is not intercepted.
        var quota = this._client.GetQuota();

        while ( quota.TryConsume() )
        {
            Console.WriteLine( $"Request sent, {quota}." );
        }
    }
}
