using Metalama.Samples.UniversalInterceptor2;

var report = new WeatherReport();

report.PrintHumidity( "Prague" );
report.PrintHumidity( "Lisbon" );
report.PrintAlerts();
await report.PrintForecastAsync( "Prague" );
report.PrintRemainingRequests();
