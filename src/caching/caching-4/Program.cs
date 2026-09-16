using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICache, Cache>()
            .AddSingleton<DatabaseFrontend>()
            .AddSingleton<ICacheKeyBuilderProvider, CacheKeyBuilderProvider>()
            .BuildServiceProvider();

        var db = serviceProvider.GetRequiredService<DatabaseFrontend>();

        Console.WriteLine( "Calling GetBlob once" );
        db.GetBlob( new BlobId( "MyContainer", new byte[] { 1, 2, 3 } ) );

        Console.WriteLine( "Calling GetBlob a second time with the same parameters." );
        db.GetBlob( new BlobId( "MyContainer", new byte[] { 1, 2, 3 } ) );

        Console.WriteLine( $"Total database operations: {db.DatabaseCalls}" );
    }
}