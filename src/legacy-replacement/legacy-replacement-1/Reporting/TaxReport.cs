namespace Reporting;

public class TaxReport
{
    private readonly LegacyTaxCalculator _taxCalculator = new();

    public decimal ComputeAnnualTax( IEnumerable<decimal> monthlyRevenues, string region )
        => this._taxCalculator.ComputeTax( monthlyRevenues.Sum(), region );

    public decimal ComputeQuarterlyTax( decimal revenue, string region )
        => this._taxCalculator.ComputeTax( revenue, region );
}
