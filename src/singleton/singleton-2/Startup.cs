using Microsoft.Extensions.DependencyInjection;

internal static class Startup
{
    public static void ConfigureServices( IServiceCollection serviceCollection )
    {
        serviceCollection
            .AddSingleton<IPerformanceCounterUploader, AwsPerformanceCounterUploader>();

        serviceCollection.AddSingleton<PerformanceCounterManager>();
    }
}