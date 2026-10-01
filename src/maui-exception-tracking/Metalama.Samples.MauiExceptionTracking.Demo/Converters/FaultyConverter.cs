using System.Globalization;

namespace Metalama.Samples.MauiExceptionTracking.Demo.Converters;

/// <summary>
/// A converter that throws an exception when the counter value exceeds 5.
/// Used to demonstrate exception tracking in bindings.
/// </summary>
public class FaultyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i && i > 5)
        {
            throw new InvalidOperationException("Counter too high!");
        }

        return value?.ToString() ?? "0";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
