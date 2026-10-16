using System.Net.Mail;

namespace Metalama.Samples.LegacyTesting1.TestSeams;

public record SentMessage( string From, string Recipients, string? Subject, string? Body );

// Replaces SmtpClient.Send in the Test configuration. It stores the messages instead of sending them.
public static class FakeMailServer
{
    public static List<SentMessage> SentMessages { get; } = new();

    // The first parameter receives the SmtpClient on which the legacy code calls Send. The other parameters have the
    // names of the parameters of SmtpClient.Send, so that they receive its arguments.
    public static void Send( SmtpClient client, string from, string recipients, string? subject, string? body )
        => SentMessages.Add( new SentMessage( from, recipients, subject, body ) );

    public static void Reset() => SentMessages.Clear();
}
