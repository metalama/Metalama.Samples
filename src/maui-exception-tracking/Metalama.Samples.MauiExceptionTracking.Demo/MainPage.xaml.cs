using Metalama.Samples.MauiExceptionTracking.Demo.ViewModels;

namespace Metalama.Samples.MauiExceptionTracking.Demo;

/// <summary>
/// Main page demonstrating exception tracking with visual context.
/// The TrackExceptions attribute automatically applies:
/// - TrackVisualContext to event handlers (captures sender as visual context)
/// - ReportExceptions to public and protected virtual methods
/// </summary>
[TrackExceptions]
public partial class MainPage : ContentPage
{
    public MainPage(DemoViewModel viewModel)
    {
        this.InitializeComponent();
        this.BindingContext = viewModel;
    }

    /// <summary>
    /// Event handler that throws an exception.
    /// The TrackExceptions aspect will automatically:
    /// 1. Set the sender (Button) as the visual context
    /// 2. Catch and report the exception to ExceptionTracker
    /// </summary>
    private void OnEventHandlerButton_Clicked(object sender, EventArgs e)
    {
        throw new InvalidOperationException("Event handler exception");
    }
}
