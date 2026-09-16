namespace Metalama.Samples.Log.Tests.SimpleMethod;

internal static class Program
{
    [Log]
    public static void Main() => SayHello();

    [Log]
    private static int SayHello()
    {
        Console.WriteLine( "Hello, world." );

        return 5;
    }
}