using System.Collections.Concurrent;

[Singleton]
public partial class PerformanceCounterManager
{
    private readonly ConcurrentDictionary<string, int> _counters = new();

    public void IncrementCounter( string name ) => this._counters.AddOrUpdate( name, 1, ( _, value ) => value + 1 );
}