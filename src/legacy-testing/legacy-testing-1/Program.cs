using Metalama.Samples.LegacyTesting1;

// InvoiceService writes the messages to this directory.
Directory.CreateDirectory( "outbox" );

Console.Write( new InvoiceService().SendReminders( 42 ) );
