using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Samples.Metrics;

public abstract class MetricAttribute : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Outbound.Select( m => m.DeclaringType ).RequireAspect<ImplementMetricsAspect>();
    }

    [Template]
    internal abstract dynamic? OverrideMethodTemplate( IField metricsField, IFieldOrProperty metricProperty );

    [Template]
    internal virtual void CreateMetricTemplate(
        IExpression meter,
        IFieldOrProperty metricProperty,
        [CompileTime] string metricName )
    {
        // Must be overridden.
    }

    internal abstract string MetricKind { get; }

    internal abstract Type MetricType { get; }
}