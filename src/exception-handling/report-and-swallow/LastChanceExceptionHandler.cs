internal class LastChanceExceptionHandler : ILastChanceExceptionHandler
{
    public void Report( Exception e )
    {
        try
        {
            // Here, we should create a full crash report and submit it
            // to the crash report server.
            Console.WriteLine( e.ToString() );
        }
        catch
        {
            // Crashes in the crash reporting process should crash the host process,
            // so they should be swallowed.
        }
    }

    public bool ShouldHandle( Exception e ) => e is not OperationCanceledException;
}