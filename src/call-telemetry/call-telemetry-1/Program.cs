using Metalama.Samples.CallTelemetry1;

var root = Directory.CreateTempSubdirectory( "call-telemetry-" ).FullName;
var reports = Directory.CreateDirectory( Path.Combine( root, "reports" ) ).FullName;

File.WriteAllText( Path.Combine( reports, "costs.csv" ), "month,amount\n2026-09,1200\n" );
File.WriteAllText( Path.Combine( reports, "sales.csv" ), "month,amount\n2026-09,4500\n" );

new BackupService().Backup( reports, Path.Combine( root, "reports.tar" ) );

Directory.Delete( root, recursive: true );
