using Metalama.Extensions.Interceptors;
using System.Runtime.CompilerServices;

// The calls in this class are not intercepted. Without this attribute, the call to the legacy calculator
// below would be redirected to this method, which would call itself.
[ExcludeInterceptors]
public static class TaxComparison
{
    // Only one call out of this number runs the new implementation, to limit the cost of the comparison.
    private const int _samplingInterval = 3;

    private static int _callCount;

    // [<snippet Compare>]
    public static decimal ComputeTax(
        LegacyTaxCalculator calculator,
        decimal amount,
        string region,
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLine = 0 )
    {
        var legacyResult = calculator.ComputeTax( amount, region );

        if ( Interlocked.Increment( ref _callCount ) % _samplingInterval != 0 )
        {
            return legacyResult;
        }

        var location = $"{callerMember}, line {callerLine}";

        try
        {
            var newResult = NewTaxCalculator.ComputeTax( amount, region );

            if ( newResult != legacyResult )
            {
                Console.WriteLine(
                    $"  Difference at {location} for {amount:F2} in {region}: legacy {legacyResult:F2}, new {newResult:F2}." );
            }
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"  The new calculator failed at {location} for {amount:F2} in {region}: {e.Message}" );
        }

        return legacyResult;
    }
    // [<endsnippet Compare>]
}
