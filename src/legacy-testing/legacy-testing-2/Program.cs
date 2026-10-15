using Metalama.Samples.LegacyTesting2;

// InvoiceService writes the messages to this directory.
Directory.CreateDirectory( "outbox" );

Console.Write( new InvoiceService().SendReminders( 42 ) );
