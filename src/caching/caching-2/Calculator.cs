public class Calculator
{
    public int InvocationCounts { get; private set; }

    [Cache]
    public int Add( int a, int b )
    {
        Console.WriteLine( "Thinking..." );
        this.InvocationCounts++;

        return a + b;
    }
}