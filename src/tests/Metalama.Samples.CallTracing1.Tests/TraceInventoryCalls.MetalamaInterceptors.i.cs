using System.Diagnostics;
using Metalama.Samples.CallTracing.Inventory;
internal static class MetalamaInterceptors
{
  internal static int InventoryClient_GetStock_Interceptor(InventoryClient receiver, string sku)
  {
    var arguments = new List<string>();
    arguments.Add($"sku: {sku}");
    var call = "OrderService.CanOrder -> InventoryClient.GetStock" + $"({string.Join(", ", arguments)})";
    var stopwatch = Stopwatch.StartNew();
    try
    {
      var result = receiver.GetStock(sku);
      Console.WriteLine($"[trace] {call} returned {result} in {stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
      return result;
    }
    catch (Exception e)
    {
      Console.WriteLine($"[trace] {call} threw {e.GetType().Name} in {stopwatch.Elapsed.TotalMilliseconds:F3} ms: {e.Message}");
      throw;
    }
  }
  internal static void InventoryClient_Reserve_Interceptor(InventoryClient receiver, string sku, int quantity)
  {
    var arguments = new List<string>();
    arguments.Add($"sku: {sku}");
    arguments.Add($"quantity: {quantity}");
    var call = "OrderService.PlaceOrder -> InventoryClient.Reserve" + $"({string.Join(", ", arguments)})";
    var stopwatch = Stopwatch.StartNew();
    try
    {
      receiver.Reserve(sku, quantity);
      object result = null;
      Console.WriteLine($"[trace] {call} succeeded in {stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
      return;
    }
    catch (Exception e)
    {
      Console.WriteLine($"[trace] {call} threw {e.GetType().Name} in {stopwatch.Elapsed.TotalMilliseconds:F3} ms: {e.Message}");
      throw;
    }
  }
}