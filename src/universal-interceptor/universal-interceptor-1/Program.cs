using Metalama.Samples.UniversalInterceptor.Weather;
using Metalama.Samples.UniversalInterceptor1;

var report = new WeatherReport();

report.Print( "Prague", TemperatureUnit.Celsius );
report.Print( "Lisbon", TemperatureUnit.Fahrenheit );

try
{
    report.Print( "Atlantis", TemperatureUnit.Celsius );
}
catch ( ArgumentException )
{
    // The log shows the failed call.
}
