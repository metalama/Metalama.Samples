namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// A XAML markup extension that creates data bindings with automatic exception tracking for value converters.
/// </summary>
/// <remarks>
/// <para>
/// Use this extension instead of standard <c>{Binding}</c> when you have a converter that might throw,
/// or when you want exception tracking on the binding.
/// </para>
/// <para>
/// XAML usage:
/// <code>
/// &lt;Label Text="{tracking:SafeBinding Counter, Converter={StaticResource MyConverter}}" /&gt;
/// &lt;Label Text="{tracking:SafeBinding Path=Value, Mode=TwoWay, StringFormat='Value: {0}'}" /&gt;
/// </code>
/// </para>
/// <para>
/// How it works:
/// <list type="number">
/// <item>Creates a standard <see cref="Binding"/> with all the specified properties</item>
/// <item>Wraps the converter (if any) in a <see cref="ValueConverterWrapper"/></item>
/// <item>The wrapper sets visual context before conversion and catches exceptions</item>
/// <item>On exception, returns the original value and reports to <see cref="IExceptionReporter"/></item>
/// </list>
/// </para>
/// <para>
/// Supports all standard binding properties: Path, Mode, StringFormat, Converter,
/// ConverterParameter, FallbackValue, TargetNullValue, and Source.
/// </para>
/// </remarks>
[ContentProperty(nameof(Path))]
[RequireService([typeof(IProvideValueTarget)])]
public partial class SafeBindingExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Gets or sets the binding path.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the binding mode.
    /// </summary>
    public BindingMode Mode { get; set; } = BindingMode.Default;

    /// <summary>
    /// Gets or sets the string format for the binding.
    /// </summary>
    public string? StringFormat { get; set; }

    /// <summary>
    /// Gets or sets the value converter.
    /// </summary>
    public IValueConverter? Converter { get; set; }

    /// <summary>
    /// Gets or sets the converter parameter.
    /// </summary>
    public object? ConverterParameter { get; set; }

    /// <summary>
    /// Gets or sets the fallback value.
    /// </summary>
    public object? FallbackValue { get; set; }

    /// <summary>
    /// Gets or sets the target null value.
    /// </summary>
    public object? TargetNullValue { get; set; }

    /// <summary>
    /// Gets or sets the binding source.
    /// </summary>
    public object? Source { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var provideValueTarget = serviceProvider.GetRequiredService<IProvideValueTarget>();
        var targetElement = provideValueTarget.TargetObject as VisualElement;

        // Services are resolved lazily by the converter when the binding runs
        var wrappedConverter = targetElement != null ? new ValueConverterWrapper(this.Converter, targetElement) : null;

        var binding = new Binding
        {
            Path = this.Path,
            Mode = this.Mode,
            StringFormat = this.StringFormat,
            Converter = wrappedConverter,
            ConverterParameter = this.ConverterParameter,
            Source = this.Source
        };

        if (this.FallbackValue != null)
        {
            binding.FallbackValue = this.FallbackValue;
        }

        if (this.TargetNullValue != null)
        {
            binding.TargetNullValue = this.TargetNullValue;
        }

        return binding;
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ProvideValue(serviceProvider);
    }
}
