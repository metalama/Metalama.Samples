using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;

// Installs the interceptor for the whole program. The tests install another interceptor in their own asynchronous flow.
CallLog.Current.Value = new CallLog();

var report = new WeatherReport();

Console.WriteLine( report.Describe( "Prague" ) );
Console.WriteLine( report.Describe( "Lisbon", TemperatureUnit.Fahrenheit ) );
Console.WriteLine( report.Describe( "Atlantis" ) );
Console.WriteLine( await report.DescribeForecastAsync( "Prague", 3 ) );
Console.WriteLine( report.DescribeHumidity( "Prague" ) );
Console.WriteLine( report.DescribeHumidity( "Lisbon" ) );
Console.WriteLine( $"Alerts: {string.Join( " ", report.GetAllAlerts() )}" );
Console.WriteLine( $"Available requests: {report.CountAvailableRequests()}" );
