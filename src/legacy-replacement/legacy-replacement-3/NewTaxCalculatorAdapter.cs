// Adapts the calls written for the legacy calculator to the new calculator.
// The instance of the legacy calculator is not used.
public static class NewTaxCalculatorAdapter
{
    public static decimal ComputeTax( LegacyTaxCalculator calculator, decimal amount, string region )
        => NewTaxCalculator.ComputeTax( amount, region );
}
