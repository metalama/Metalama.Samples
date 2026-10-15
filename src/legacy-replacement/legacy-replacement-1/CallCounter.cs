using Metalama.Extensions.Interceptors;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

// The calls in this class are not intercepted. Without this attribute, the call to the legacy calculator
// below would be redirected to this method, which would call itself.
[ExcludeInterceptors]
public static class CallCounter
{
    private static readonly ConcurrentDictionary<string, int> _counts = new();

    static CallCounter()
    {
        AppDomain.CurrentDomain.ProcessExit += ( _, _ ) => PrintCounts();
    }

    public static decimal ComputeTax(
        LegacyTaxCalculator calculator,
        decimal amount,
        string region,
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLine = 0 )
    {
        var location = $"{callerMember}, line {callerLine}";
        _counts.AddOrUpdate( location, 1, ( _, count ) => count + 1 );

        return calculator.ComputeTax( amount, region );
    }

    private static void PrintCounts()
    {
        Console.WriteLine();
        Console.WriteLine( "Calls to LegacyTaxCalculator.ComputeTax:" );

        foreach ( var (location, count) in _counts.OrderBy( p => p.Key ) )
        {
            Console.WriteLine( $"  {location}: {count} call(s)" );
        }
    }
}
