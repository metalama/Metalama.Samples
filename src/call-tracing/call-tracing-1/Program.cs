using Metalama.Samples.CallTracing1;

var orders = new OrderService();

if ( orders.CanOrder( "BOOK", 3 ) )
{
    orders.PlaceOrder( "BOOK", 3 );
}

try
{
    orders.PlaceOrder( "PEN", 5 );
}
catch ( InvalidOperationException )
{
    // The trace shows the failed call.
}
