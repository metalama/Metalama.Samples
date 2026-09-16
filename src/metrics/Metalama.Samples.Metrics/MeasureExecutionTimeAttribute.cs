using System.Diagnostics;
using System.Diagnostics.Metrics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Samples.Metrics;

public class MeasureExecutionTimeAttribute : MetricAttribute
{
    internal override dynamic? OverrideMethodTemplate( IField metricsField, IFieldOrProperty metricProperty )
    {
        var meterExpression = metricProperty.WithObject( metricsField ).WithOptions( InvokerOptions.NullConditional );

        var timestamp = Stopwatch.GetTimestamp();

        try
        {
            return meta.Proceed();
        }
        finally
        {
            ((Counter<long>) meterExpression.Value!).Add( Stopwatch.GetTimestamp() - timestamp );
        }
    }

    internal override void CreateMetricTemplate(
        IExpression meter,
        IFieldOrProperty metricProperty,
        [CompileTime] string metricName )
    {
        metricProperty.Value = ((Meter) meter.Value!).CreateCounter<long>( metricName );
    }

    internal override string MetricKind => "ExecutionTime";

    internal override Type MetricType => typeof(Counter<long>);
}