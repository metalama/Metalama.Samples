namespace Metalama.Samples.LegacyTesting1.TestSeams;

// Replaces Thread.Sleep in the Test configuration. It adds the time to a counter instead of waiting.
public static class FakeClock
{
    public static TimeSpan Elapsed { get; private set; }

    public static void Sleep( int millisecondsTimeout ) => Elapsed += TimeSpan.FromMilliseconds( millisecondsTimeout );

    public static void Reset() => Elapsed = TimeSpan.Zero;
}
