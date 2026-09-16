using System.Diagnostics.Metrics;
using System.Reflection;

namespace Metalama.Samples.Metrics;

public interface IMetricHost
{
    string ApplicationName { get; }

    string? ApplicationVersion { get; }

    IEnumerable<KeyValuePair<string, object?>> Tags { get; }

    void RegisterInstrument( Instrument instrument, MethodInfo method, string metricKind );
}