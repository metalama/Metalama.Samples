using Metalama.Framework.Code;

namespace Metalama.Samples.Metrics;

internal class MetricTypeAnnotation : IAnnotation<ICompilation>
{
    public IDurableRef<INamedType> MetricType { get; }

    public MetricTypeAnnotation( IDurableRef<INamedType> metricType )
    {
        this.MetricType = metricType;
    }
}