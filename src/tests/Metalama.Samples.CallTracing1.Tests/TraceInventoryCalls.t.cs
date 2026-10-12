using Metalama.Samples.CallTracing.Inventory;
namespace Metalama.Samples.CallTracing1.Tests.TraceInventoryCalls;
public class OrderService
{
  private readonly InventoryClient _inventory = new();
  public bool CanOrder(string sku, int quantity) => MetalamaInterceptors.InventoryClient_GetStock_Interceptor(this._inventory, sku: sku) >= quantity;
  public void PlaceOrder(string sku, int quantity)
  {
    MetalamaInterceptors.InventoryClient_Reserve_Interceptor(this._inventory, sku: sku, quantity: quantity);
  }
}