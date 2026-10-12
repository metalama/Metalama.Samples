public static class NewTaxCalculator
{
    public static decimal ComputeTax( decimal amount, string region )
        => region switch
        {
            "EU" => amount * 0.2m,
            "CH" => amount * 0.081m,
            _ => amount * 0.1m
        };
}
