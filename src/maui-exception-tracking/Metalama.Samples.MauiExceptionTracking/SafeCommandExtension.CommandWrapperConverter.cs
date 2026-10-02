using System.Globalization;
using System.Windows.Input;

namespace Metalama.Samples.MauiExceptionTracking;

public partial class SafeCommandExtension
{
    /// <summary>
    /// Value converter that wraps <see cref="ICommand"/> values in <see cref="CommandWrapper"/>
    /// to enable exception tracking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This converter is used by <see cref="SafeCommandExtension"/> to intercept the command
    /// value from the binding. Services (<see cref="IVisualContextTracker"/> and
    /// <see cref="IExceptionReporter"/>) are resolved lazily from the target element's
    /// MAUI service provider when the converter runs, not during XAML parsing.
    /// </para>
    /// <para>
    /// If services are not available (e.g., before the app is fully initialized),
    /// the original command is returned unwrapped for graceful degradation.
    /// </para>
    /// </remarks>
    private sealed class CommandWrapperConverter : IValueConverter
    {
        private readonly VisualElement _targetElement;

        public CommandWrapperConverter(VisualElement targetElement)
        {
            this._targetElement = targetElement;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not ICommand command)
            {
                return value;
            }

            // Resolve services lazily from the element
            var mauiServices = this._targetElement.GetMauiServiceProvider();
            if (mauiServices == null)
            {
                return command;
            }

            var visualContextTracker = mauiServices.GetService<IVisualContextTracker>();
            var exceptionReporter = mauiServices.GetService<IExceptionReporter>();

            // If services aren't available, return the command unwrapped
            if (visualContextTracker == null || exceptionReporter == null)
            {
                return command;
            }

            return new CommandWrapper(command, this._targetElement, visualContextTracker, exceptionReporter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
