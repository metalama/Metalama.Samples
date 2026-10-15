namespace Metalama.Samples.Metrics.Demo;

public class HatShop
{
    private int _orderCount;

    [MeasureExecutionCount]
    [MeasureExecutionTime]
    [MeasureExceptionCount]
    public void PlaceOrder()
    {
        this._orderCount++;

        if ( this._orderCount % 10 == 0 )
        {
            throw new InvalidOperationException( "The hat is out of stock." );
        }

        Console.WriteLine( "Ordering a hat." );
    }
}
