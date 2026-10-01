namespace Metalama.Samples.LogParameters.Tests.Normal;

internal class Foo
{
    [Log]
    private void Bar( int a, string b ) => Console.WriteLine( $"a={a}, b='{b}'" );
}