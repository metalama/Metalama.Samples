using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NameGenerator;
using NameGenerator.Generators;
using System.Windows;

public partial class App
{
    private static IHost? _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices( ( _, services ) =>
            {
                // Add windows
                services.AddSingleton<MainWindow>();

                // Add root view models
                services.AddSingleton<MainViewModel>();

                // Add services
                services.AddSingleton<IMementoCaretaker, Caretaker>();
                services.AddSingleton<IFishGenerator, FishGenerator>();
                services.AddSingleton<GeneratorBase, RealNameGenerator>();
            } )
            .Build();
    }

    protected override async void OnStartup( StartupEventArgs e )
    {
        await _host!.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup( e );
    }

    protected override async void OnExit( ExitEventArgs e )
    {
        await _host!.StopAsync();
        base.OnExit( e );
    }
}