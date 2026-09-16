using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;

namespace App;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = BuildServices();
        var activity = serviceProvider.GetRequiredService<Activity>();

        activity.DoOperations();
    }

    private static ServiceProvider BuildServices()
    {
        var serviceCollection = new ServiceCollection()
            .AddLogging(opt => opt.AddConsole().SetMinimumLevel(LogLevel.Trace))
            .AddSingleton<Repository<Person>>()
            .AddSingleton<Activity>();

        return serviceCollection.BuildServiceProvider();
    }
}