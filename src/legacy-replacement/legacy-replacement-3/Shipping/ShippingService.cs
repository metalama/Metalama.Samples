namespace Shipping;

public class ShippingService
{
    private readonly LegacyTaxCalculator _taxCalculator = new();

    public decimal ComputeShippingCost( decimal weight, string region )
    {
        var cost = 5 + (weight * 2);

        return cost + this._taxCalculator.ComputeTax( cost, region );
    }
}
