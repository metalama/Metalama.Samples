using Metalama.Framework.Aspects;

public class RetryAttribute : OverrideMethodAspect
{
    /// <summary>
    /// Gets or sets the maximum number of times that the method should be executed.
    /// </summary>
    public int Attempts { get; set; } = 3;

    /// <summary>
    /// Gets or set the delay, in ms, to wait between the first and the second attempt.
    /// The delay is doubled at every further attempt.
    /// </summary>
    public double Delay { get; set; } = 100;

    public override dynamic? OverrideMethod()
    {
        for ( var i = 0;; i++ )
        {
            try
            {
                return meta.Proceed();
            }
            catch ( Exception e ) when ( i < this.Attempts )
            {
                var delay = this.Delay * Math.Pow( 2, i + 1 );
                Console.WriteLine( e.Message + $" Waiting {delay} ms." );
                Thread.Sleep( (int) delay );
            }
        }
    }
}