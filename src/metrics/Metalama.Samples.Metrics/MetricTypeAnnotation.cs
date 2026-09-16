using Metalama.Framework.Code;

namespace Metalama.Samples.Metrics;

internal class MetricTypeAnnotation : IAnnotation<ICompilation>
{
    public IRef<INamedType> MetricType { get; }

    public MetricTypeAnnotation( IRef<INamedType> metricType )
    {
        this.MetricType = metricType;
    }
}