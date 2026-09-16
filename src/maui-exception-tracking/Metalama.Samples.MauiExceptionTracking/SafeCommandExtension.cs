namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// A XAML markup extension that creates command bindings with automatic exception tracking.
/// </summary>
/// <remarks>
/// <para>
/// Use this extension instead of standard <c>{Binding}</c> for command properties to enable
/// automatic exception tracking with visual context.
/// </para>
/// <para>
/// XAML usage:
/// <code>
/// &lt;Button Command="{tracking:SafeCommand MyCommand}" /&gt;
/// &lt;Button Command="{tracking:SafeCommand Path=MyCommand, Source={StaticResource ViewModel}}" /&gt;
/// </code>
/// </para>
/// <para>
/// How it works:
/// <list type="number">
/// <item>Creates a <see cref="Binding"/> to the specified command property</item>
/// <item>Uses a <see cref="CommandWrapperConverter"/> that wraps the command in a <see cref="CommandWrapper"/></item>
/// <item>The wrapper captures the visual element, sets visual context before execution, and catches exceptions</item>
/// <item>Services (<see cref="IVisualContextTracker"/>, <see cref="IExceptionReporter"/>) are resolved lazily at runtime</item>
/// </list>
/// </para>
/// <para>
/// Both synchronous and asynchronous commands are supported. For <c>AsyncRelayCommand</c> from
/// CommunityToolkit.Mvvm, the wrapper properly awaits the command and catches async exceptions.
/// </para>
/// </remarks>
[ContentProperty(nameof(Path))]
[RequireService([typeof(IProvideValueTarget)])]
public partial class SafeCommandExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Gets or sets the binding path to the command.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the binding source.
    /// </summary>
    public object? Source { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var provideValueTarget = serviceProvider.GetRequiredService<IProvideValueTarget>();

        var targetElement = provideValueTarget.TargetObject as VisualElement;

        // Services are resolved lazily by the converter when the binding runs
        return new Binding
        {
            Path = this.Path,
            Source = this.Source,
            Converter = new CommandWrapperConverter(targetElement)
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return this.ProvideValue(serviceProvider);
    }
}
