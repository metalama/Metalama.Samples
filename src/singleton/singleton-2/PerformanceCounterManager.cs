using System.Collections.Concurrent;
using System.Diagnostics;

[Singleton]
public class PerformanceCounterManager
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private readonly ConcurrentDictionary<string, int> _counters = new();

    private readonly IPerformanceCounterUploader _uploader;

    public PerformanceCounterManager( IPerformanceCounterUploader uploader )
    {
        this._uploader = uploader;
    }

    public void IncrementCounter( string name ) => this._counters.AddOrUpdate( name, 1, ( _, value ) => value + 1 );

    public void UploadAndReset()
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
            this._uploader.UploadCounter( counter.Key, counter.Value / elapsed.TotalSeconds );
        }
    }
}