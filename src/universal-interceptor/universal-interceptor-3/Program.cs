using Metalama.Samples.UniversalInterceptor3;

// Installs the interceptor for the whole program. Tests install another interceptor in their own asynchronous flow.
CallLog.Current.Value = new CallLog();

var report = new WeatherReport();

Console.WriteLine( report.Describe( "Prague" ) );
Console.WriteLine( report.Describe( "Atlantis" ) );
Console.WriteLine( await report.DescribeForecastAsync( "Lisbon", 5 ) );
