namespace Metalama.Samples.Caching2.Tests.Eligibility;

public class Calculator
{
    private int? _value;

    // Eligible.
    [Cache]
    public int Add( int a, int b ) => a + b;

    // Ineligible because it is void.
    [Cache]
    public void SetValue( int value ) => this._value = value;

    // Ineligible because it has an out parameter.
    [Cache]
    public bool TryGetValue( out int? value )
    {
        value = this._value;

        return value.HasValue;
    }
}