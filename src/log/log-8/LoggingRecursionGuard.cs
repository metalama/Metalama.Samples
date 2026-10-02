internal static class LoggingRecursionGuard
{
    [ThreadStatic]
    public static bool IsLogging;

    public static DisposeCookie Begin()
    {
        if ( IsLogging )
        {
            return new DisposeCookie( false );
        }
        else
        {
            IsLogging = true;

            return new DisposeCookie( true );
        }
    }

    internal class DisposeCookie : IDisposable
    {
        public DisposeCookie( bool canLog )
        {
            this.CanLog = canLog;
        }

        public bool CanLog { get; }

        public void Dispose()
        {
            if ( this.CanLog )
            {
                IsLogging = false;
            }
        }
    }
}