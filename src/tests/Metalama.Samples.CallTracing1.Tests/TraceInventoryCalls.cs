#if TEST_OPTIONS
// @Include(../../call-tracing/call-tracing-1/TracingFabric.cs)
#endif

using Metalama.Samples.CallTracing.Inventory;

namespace Metalama.Samples.CallTracing1.Tests.TraceInventoryCalls;

public class OrderService
{
    private readonly InventoryClient _inventory = new();

    public bool CanOrder( string sku, int quantity ) => this._inventory.GetStock( sku ) >= quantity;

    public void PlaceOrder( string sku, int quantity )
    {
        this._inventory.Reserve( sku, quantity );
    }
}
