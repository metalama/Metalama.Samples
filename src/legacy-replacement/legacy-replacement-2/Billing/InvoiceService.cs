namespace Billing;

public class InvoiceService
{
    private readonly LegacyTaxCalculator _taxCalculator = new();

    public decimal ComputeTotal( decimal amount, string region )
        => amount + this._taxCalculator.ComputeTax( amount, region );

    public decimal ComputeRefund( decimal amount, string region )
        => -(amount + this._taxCalculator.ComputeTax( amount, region ));
}
