using Metalama.Samples.LegacyTesting3.Recording;
using Xunit;

namespace Metalama.Samples.LegacyTesting3.UnitTests;

// Pins the current behavior of InvoiceService, as it was recorded in production, before the code is refactored.
public class InvoiceServiceCharacterizationTests
{
    [Fact]
    public void SendReminders()
    {
        // The recording was made by running the program in the Record configuration.
        InteractionReplayer.Load( File.ReadAllText( Path.Combine( "Recordings", "send-reminders-42.json" ) ) );

        var summary = new InvoiceService().SendReminders( 42 );

        // The replayer has checked each call and its arguments, including the messages. All of them must be made.
        Assert.Equal( 0, InteractionReplayer.PendingInteractionCount );

        Snapshot.Verify( summary, "SendReminders" );
    }
}
