using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using System.Linq;

namespace Metalama.Samples.MauiExceptionTracking;


/// <summary>
/// A Metalama type-level aspect that automatically instruments methods for exception tracking.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a class (typically a MAUI page or view) to automatically
/// add exception tracking to its methods.
/// </para>
/// <para>
/// Usage:
/// <code>
/// [TrackExceptions]
/// public partial class MainPage : ContentPage
/// {
///     // Event handlers get visual context tracking + exception swallowing
///     private void OnButton_Clicked(object sender, EventArgs e)
///     {
///         // Exceptions are caught, reported, and swallowed
///     }
///
///     // Public methods get exception reporting (rethrows)
///     public void DoWork()
///     {
///         // Exceptions are caught, reported, then rethrown
///     }
/// }
/// </code>
/// </para>
/// <para>
/// Method categorization:
/// <list type="bullet">
/// <item><b>Event handlers</b> (signature: <c>void Method(object, *EventArgs)</c>) -
/// Visual context is captured from the sender parameter, exceptions are swallowed after reporting</item>
/// <item><b>Public instance methods</b> - Exceptions are reported and rethrown</item>
/// <item><b>Protected virtual methods</b> - Exceptions are reported and rethrown</item>
/// </list>
/// </para>
/// <para>
/// This aspect uses <see cref="IntroduceDependencyAttribute"/> to inject
/// <see cref="IExceptionReporter"/> and <see cref="IVisualContextTracker"/> into the target type.
/// </para>
/// <para>
/// The <see cref="InheritableAttribute"/> ensures this aspect is inherited by derived types.
/// </para>
/// </remarks>
[Inheritable]
public class TrackExceptionsAttribute : TypeAspect
{
    /// <summary>
    /// Warning reported when a protected override method is not sealed, which may cause
    /// duplicate exception handling in derived types.
    /// </summary>
    private static readonly DiagnosticDefinition<IMethod> _unsealedOverrideWarning = new(
        "MAUI0001",
        Severity.Warning,
        "Method '{0}' overrides a Microsoft framework method but is not sealed. " +
        "Consider sealing this method to prevent inconsistent exception handling in derived types.");

    /// <summary>
    /// Error reported when [EntryPoint] is applied to a static method.
    /// </summary>
    private static readonly DiagnosticDefinition<IMethod> _staticEntryPointError = new(
        "MAUI0002",
        Severity.Error,
        "[EntryPoint] cannot be applied to static method '{0}'. " +
        "Exception tracking requires an instance context to access the visual element.");

    [IntroduceDependency]
    private readonly IExceptionReporter _exceptionReporter;

    [IntroduceDependency]
    private readonly IVisualContextTracker _visualContextTracker;

    /// <inheritdoc />
    public override void BuildEligibility( IEligibilityBuilder<INamedType> builder )
    {
        base.BuildEligibility( builder );

        // Only apply to types convertible to VisualElement.
        builder.MustBeConvertibleTo( typeof( VisualElement ) );
    }

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var method in builder.Target.Methods)
        {
            // Skip property accessors, static methods we don't want to track, etc.
            if (method.IsImplicitlyDeclared)
            {
                continue;
            }

            var methodAdviser = builder.With(method);

            // Check if this is an event handler pattern: void Method(object sender, *EventArgs)
            if (IsEventHandlerSignature(method))
            {
                // Event handlers get visual context tracking and exception swallowing
                methodAdviser.Override( nameof(this.EventHandlerTemplate));
            }
            // Override public methods
            else if (method.Accessibility == Accessibility.Public && !method.IsStatic)
            {
                methodAdviser.Override( nameof(this.MethodTemplate));
            }
            // Override methods that might be invoked by the MAUI framework.
            else if (method.Accessibility == Accessibility.Protected
                && method.IsOverride
                && method.OverriddenMethod!.DeclaringType.ContainingNamespace.FullName.StartsWith("Microsoft") )
            {
                // Report warning if method is not sealed - exception handling may be duplicated in derived types,
                // or may incorrectly believe the call to base.Method() was successful when it actually threw and then swallowed.
                if ( !method.IsSealed)
                {
                    builder.Diagnostics.Report(_unsealedOverrideWarning.WithArguments(method), method);
                }

                methodAdviser.Override(nameof(this.MethodTemplate));
            }
            // Override other methods marked with [EntryPoint]
            else if (method.Attributes.OfAttributeType( typeof(EntryPointAttribute)).Any() )
            {
                // Report error if method is static - we need instance context
                if (method.IsStatic)
                {
                    builder.Diagnostics.Report(_staticEntryPointError.WithArguments(method), method);
                    continue;
                }

                methodAdviser.Override(nameof(this.MethodTemplate));
            }
        }
    }

    /// <summary>
    /// Template for event handlers - tracks visual context and swallows exceptions.
    /// </summary>
    [Template]
    private void EventHandlerTemplate()
    {
        VisualElement? visualElement = null;

        // Get sender parameter for visual context
        if ( meta.Target.Parameters.Count >= 1 )
        {
            // Disputable design choice: if sender is a VisualElement, we switch to its context (instead of our).
            // Hopefully this will be a _child_ of the current visual element, sharing the same root context.
            visualElement = meta.Target.Parameters[0].Value as VisualElement;            
        }

        // Switch to the execution context of the _sender_ visual element.
        // If visualElement null, no context is pushed.
        using var scope = this._visualContextTracker.Push( visualElement );

        try
        {
            meta.Proceed();
        }
        catch ( Exception ex )
        {
            this._exceptionReporter.Report( ex, this._visualContextTracker.Current );
            // Swallow exception.
        }

    }

    /// <summary>
    /// Template for regular methods - reports exceptions but rethrows.
    /// </summary>
    [Template]
    private dynamic? MethodTemplate()
    {
        using var scope = this._visualContextTracker.Push( (VisualElement) meta.This );

        try
        {
            return meta.Proceed();
        }
        catch (Exception ex)
        {
            this._exceptionReporter.Report(ex, (VisualElement) meta.This );

            if ( meta.Target.Method.ReturnType.Equals( SpecialType.Void ) )
            {
                // For void methods, swallow the exception by returning.
                return null;
            }
            else
            {
                throw;
            }
        }
    }

    private static bool IsEventHandlerSignature(IMethod method)
    {
        // Check: void return type
        if (!method.ReturnType.Equals(SpecialType.Void))
        {
            return false;
        }

        // Check: exactly 2 parameters
        if (method.Parameters.Count != 2)
        {
            return false;
        }

        // Check: second parameter is an EventArgs.
        var secondParam = method.Parameters[1];

        if (!secondParam.Type.IsConvertibleTo(typeof(EventArgs)))
        {
            return false;
        }

        return true;
    }
}
