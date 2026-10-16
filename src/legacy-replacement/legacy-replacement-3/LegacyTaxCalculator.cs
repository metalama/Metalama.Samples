public class LegacyTaxCalculator
{
    public decimal ComputeTax( decimal amount, string region )
        => region switch
        {
            "EU" => amount * 0.2m,
            _ => amount * 0.1m
        };
}
