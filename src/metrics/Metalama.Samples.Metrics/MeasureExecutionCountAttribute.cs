using System.Diagnostics.Metrics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Samples.Metrics;

public class MeasureExecutionCountAttribute : MetricAttribute
{
    internal override dynamic? OverrideMethodTemplate( IField metricsField, IFieldOrProperty metricProperty )
    {
        var meterExpression = metricProperty.WithObject( metricsField ).WithOptions( InvokerOptions.NullConditional );

        ((Counter<long>) meterExpression.Value!).Add( 1 );

        return meta.Proceed();
    }

    internal override void CreateMetricTemplate(
        IExpression meter,
        IFieldOrProperty metricProperty,
        [CompileTime] string metricName )
    {
        metricProperty.Value = ((Meter) meter.Value!).CreateCounter<long>( metricName );
    }

    internal override string MetricKind => "ExecutionCount";

    internal override Type MetricType => typeof(Counter<long>);
}