using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor3;

public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public string Describe( string city )
    {
        try
        {
            var temperature = this._client.GetTemperature( city );

            return this._client.TryGetHumidity( city, out var humidity )
                ? $"{city}: {temperature} {this._client.Unit}, humidity {humidity} %"
                : $"{city}: {temperature} {this._client.Unit}";
        }
        catch ( ArgumentException )
        {
            return $"{city}: unknown city";
        }
    }

    public async Task<string> DescribeForecastAsync( string city, int days )
    {
        var forecast = await this._client.GetForecastAsync( city, days );

        return $"{city}: {forecast.AverageHigh} on average in the next {forecast.Days} days";
    }
}
