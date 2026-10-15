using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor1;

public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public void Print( string city, TemperatureUnit unit )
    {
        this._client.SetUnit( unit );
        var temperature = this._client.GetTemperature( city );

        Console.WriteLine( $"{city}: {temperature} {unit}" );
    }
}
