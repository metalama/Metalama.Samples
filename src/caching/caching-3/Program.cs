using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICache, Cache>()
            .AddSingleton<DatabaseFrontend>()
            .BuildServiceProvider();

        var db = serviceProvider.GetRequiredService<DatabaseFrontend>();

        Console.WriteLine( "Calling GetEntity once" );
        db.GetEntity( new EntityKey( "Invoice", 1 ) );

        Console.WriteLine( "Calling GetEntity a second time with the same parameters." );
        db.GetEntity( new EntityKey( "Invoice", 1 ) );

        Console.WriteLine( "Calling GetEntity a third time with the different parameters." );
        db.GetEntity( new EntityKey( "Invoice", 2 ) );

        Console.WriteLine( $"Total database operations: {db.DatabaseCalls}" );
    }
}