using System.Globalization;

namespace Metalama.Samples.MauiExceptionTracking;

public partial class SafeBindingExtension
{
    /// <summary>
    /// Wraps an <see cref="IValueConverter"/> to provide visual context tracking and exception handling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This wrapper intercepts calls to <see cref="IValueConverter.Convert"/> and
    /// <see cref="IValueConverter.ConvertBack"/> to:
    /// <list type="bullet">
    /// <item>Push the target visual element onto the <see cref="IVisualContextTracker"/></item>
    /// <item>Catch exceptions and report them to <see cref="IExceptionReporter"/></item>
    /// <item>Return the original value on exception (graceful degradation)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Services are resolved lazily from the target element's MAUI service provider
    /// when the converter runs, not during XAML parsing. If services are not available,
    /// the inner converter is called without tracking.
    /// </para>
    /// </remarks>
    private sealed class ValueConverterWrapper : IValueConverter
    {
        private readonly IValueConverter? _innerConverter;
        private readonly VisualElement _targetElement;

        public ValueConverterWrapper(IValueConverter? innerConverter, VisualElement targetElement)
        {
            this._innerConverter = innerConverter;
            this._targetElement = targetElement;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var (visualContextTracker, exceptionReporter) = this.GetServices();

            // If services aren't available, just call the inner converter without tracking
            if (visualContextTracker == null || exceptionReporter == null)
            {
                return this._innerConverter?.Convert(value, targetType, parameter, culture) ?? value;
            }

            using var context = visualContextTracker.Push(this._targetElement);

            try
            {
                if (this._innerConverter != null)
                {
                    return this._innerConverter.Convert(value, targetType, parameter, culture);
                }
                return value;
            }
            catch (Exception ex)
            {
                exceptionReporter.Report(ex, this._targetElement, "Value converter Convert");
                return value; // Return original value on failure
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var (visualContextTracker, exceptionReporter) = this.GetServices();

            // If services aren't available, just call the inner converter without tracking
            if (visualContextTracker == null || exceptionReporter == null)
            {
                return this._innerConverter?.ConvertBack(value, targetType, parameter, culture) ?? value;
            }

            using var context = visualContextTracker.Push(this._targetElement);

            try
            {
                if (this._innerConverter != null)
                {
                    return this._innerConverter.ConvertBack(value, targetType, parameter, culture);
                }
                return value;
            }
            catch (Exception ex)
            {
                exceptionReporter.Report(ex, this._targetElement, "Value converter ConvertBack");
                return value; // Return original value on failure
            }
        }

        private (IVisualContextTracker?, IExceptionReporter?) GetServices()
        {
            var mauiServices = this._targetElement.GetMauiServiceProvider();
            var visualContextTracker = mauiServices?.GetService<IVisualContextTracker>();
            var exceptionReporter = mauiServices?.GetService<IExceptionReporter>();
            return (visualContextTracker, exceptionReporter);
        }
    }
}
