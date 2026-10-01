using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IPartProvider, PartProvider>()
            .AddSingleton<ILastChanceExceptionHandler, LastChanceExceptionHandler>()
            .BuildServiceProvider();

        var service = serviceProvider.GetService<IPartProvider>()!;
        service.GetPart( "main" );

        Console.WriteLine( "The program ended gracefully." );
    }
}