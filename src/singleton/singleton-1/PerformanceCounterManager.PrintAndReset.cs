using System.Diagnostics;

public partial class PerformanceCounterManager
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void PrintAndReset()
    {
        Dictionary<string, int> oldCounters;
        TimeSpan elapsed;

        lock ( this._stopwatch )
        {
            oldCounters = this._counters.RemoveAll();

            elapsed = this._stopwatch.Elapsed;
            this._stopwatch.Restart();
        }

        foreach ( var counter in oldCounters )
        {
            Console.WriteLine( $"{counter.Key}: {counter.Value / elapsed.TotalSeconds:f2} calls/s" );
        }
    }
}