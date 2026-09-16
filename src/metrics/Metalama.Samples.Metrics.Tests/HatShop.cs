using Metalama.Samples.Metrics;

#pragma warning disable CA2201

[assembly: GenerateAddMetricsExtension]

namespace Metalama.Samples.Metrics.Example;

public class HatShop
{
    private int _executionCount;

    [MeasureExecutionCount]
    [MeasureExecutionTime]
    [MeasureExceptionCount]
    public void PlaceOrder()
    {
        this._executionCount++;

        if ( this._executionCount % 10 == 0 )
        {
            throw new Exception();
        }
        else
        {
            Console.WriteLine( "Ordering a hat." );
        }
    }
}