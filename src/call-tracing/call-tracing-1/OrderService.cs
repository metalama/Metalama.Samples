using Metalama.Samples.CallTracing.Inventory;

namespace Metalama.Samples.CallTracing1;

public class OrderService
{
    private readonly InventoryClient _inventory = new();

    public bool CanOrder( string sku, int quantity ) => this._inventory.GetStock( sku ) >= quantity;

    public void PlaceOrder( string sku, int quantity )
    {
        this._inventory.Reserve( sku, quantity );
    }
}
