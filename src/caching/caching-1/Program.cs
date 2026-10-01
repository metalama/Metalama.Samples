using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<Calculator>()
            .AddSingleton<ICache, Cache>()
            .BuildServiceProvider();

        var calculator = serviceProvider.GetRequiredService<Calculator>();

        Console.WriteLine( "First call." );
        calculator.Add( 1, 2 );

        Console.WriteLine( "Second call." );
        calculator.Add( 1, 2 );

        Console.WriteLine( $"Total real executions: {calculator.InvocationCounts}" );
    }
}