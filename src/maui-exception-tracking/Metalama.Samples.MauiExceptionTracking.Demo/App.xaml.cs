namespace Metalama.Samples.MauiExceptionTracking.Demo;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
        this.InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var mainPage = this._serviceProvider.GetRequiredService<MainPage>();
        return new Window(new NavigationPage(mainPage));
    }
}
