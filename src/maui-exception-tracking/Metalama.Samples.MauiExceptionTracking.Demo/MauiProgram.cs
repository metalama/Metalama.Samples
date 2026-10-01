using Metalama.Samples.MauiExceptionTracking.Demo.ViewModels;

namespace Metalama.Samples.MauiExceptionTracking.Demo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseVisualContextTracking()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register exception tracker implementation (both concrete and interface resolve to same singleton)
        builder.Services.AddSingleton<ExceptionTracker>();
        builder.Services.AddSingleton<IExceptionReporter>(sp => sp.GetRequiredService<ExceptionTracker>());

        // Register pages and view models
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<DemoViewModel>();

        return builder.Build();
    }
}
