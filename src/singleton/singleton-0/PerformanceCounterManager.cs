using System.Collections.Concurrent;

public partial class PerformanceCounterManager
{
    private readonly ConcurrentDictionary<string, int> _counters = new();

    private PerformanceCounterManager() { }

    public static PerformanceCounterManager Instance { get; } = new();

    public void IncrementCounter( string name ) => this._counters.AddOrUpdate( name, 1, ( _, value ) => value + 1 );
}