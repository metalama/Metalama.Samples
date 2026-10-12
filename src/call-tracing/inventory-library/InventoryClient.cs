namespace Metalama.Samples.CallTracing.Inventory;

// A client of an inventory service. In a real application, this class would be part of a library that you do not own.
public class InventoryClient
{
    private readonly Dictionary<string, int> _stock = new() { ["BOOK"] = 10, ["PEN"] = 2 };

    public int GetStock( string sku )
        => this._stock.TryGetValue( sku, out var quantity )
            ? quantity
            : throw new ArgumentException( $"Unknown SKU '{sku}'.", nameof(sku) );

    public void Reserve( string sku, int quantity )
    {
        var stock = this.GetStock( sku );

        if ( quantity > stock )
        {
            throw new InvalidOperationException( $"Only {stock} items of '{sku}' are in stock." );
        }

        this._stock[sku] = stock - quantity;
    }
}
