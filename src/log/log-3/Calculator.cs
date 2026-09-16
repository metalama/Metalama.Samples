internal static class Calculator
{
    [Log]
    public static double Add( double a, double b ) => a + b;

    [Log]
    public static double Divide( double a, double b ) => a / b;

    [Log]
    public static void IntegerDivide( int a, int b, out int quotient, out int remainder )
    {
        quotient = a / b;
        remainder = a % b;
    }
}