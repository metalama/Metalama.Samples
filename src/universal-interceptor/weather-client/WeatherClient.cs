namespace Metalama.Samples.UniversalInterceptor.Weather;

// A client of a weather service. In a real application, this class would be part of a NuGet package that you do not own.
// It returns fixed values so that the example runs without a network connection.
public class WeatherClient
{
    private readonly Dictionary<string, double> _temperatures = new() { ["Prague"] = 12.5, ["Lisbon"] = 21.0 };
    private readonly Dictionary<string, int> _humidities = new() { ["Prague"] = 71 };

    private readonly string[] _alerts =
    [
        "Lisbon: strong wind on Monday.", "Prague: frost on Tuesday.", "Prague: heavy rain on Thursday."
    ];

    public TemperatureUnit Unit { get; private set; } = TemperatureUnit.Celsius;

    public void SetUnit( TemperatureUnit unit ) => this.Unit = unit;

    public double GetTemperature( string city )
    {
        var celsius = this._temperatures.TryGetValue( city, out var temperature )
            ? temperature
            : throw new ArgumentException( $"Unknown city '{city}'.", nameof(city) );

        return this.Unit == TemperatureUnit.Celsius ? celsius : (celsius * 9 / 5) + 32;
    }

    public bool TryGetHumidity( string city, out int humidity ) => this._humidities.TryGetValue( city, out humidity );

    // Returns the next page of alerts, and moves the cursor after the returned alerts.
    public string[] GetAlerts( ref int cursor, int pageSize )
    {
        var page = this._alerts.Skip( cursor ).Take( pageSize ).ToArray();
        cursor += page.Length;

        return page;
    }

    public async Task<Forecast> GetForecastAsync( string city, int days, CancellationToken cancellationToken = default )
    {
        await Task.Delay( 10, cancellationToken );

        return new Forecast( city, days, this.GetTemperature( city ) + 1 );
    }

    // Completes synchronously when the value is in the cache of the client, so the method returns a ValueTask.
    public ValueTask<double> GetTemperatureAsync( string city, CancellationToken cancellationToken = default )
        => ValueTask.FromResult( this.GetTemperature( city ) );

    public async Task RefreshAsync( CancellationToken cancellationToken = default )
    {
        await Task.Delay( 10, cancellationToken );
    }

    // Completes synchronously when the cache is already empty, so the method returns a ValueTask.
    public ValueTask ClearCacheAsync( CancellationToken cancellationToken = default ) => ValueTask.CompletedTask;

    public RequestQuota GetQuota() => new( 2 );
}
