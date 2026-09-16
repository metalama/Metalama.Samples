using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Samples.MauiExceptionTracking;

/// <summary>
/// A Metalama method-level aspect that wraps methods in try/catch and reports exceptions.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to individual methods to add exception tracking.
/// For type-wide exception tracking, use <see cref="TrackExceptionsAttribute"/> instead.
/// </para>
/// <para>
/// Usage:
/// <code>
/// // Report and rethrow (default)
/// [ReportExceptions]
/// public void DoWork()
/// {
///     // Exceptions are caught, reported to IExceptionReporter, then rethrown
/// }
///
/// // Report and swallow
/// [ReportExceptions(Swallow = true)]
/// public void DoWorkSafe()
/// {
///     // Exceptions are caught, reported, and swallowed (method returns default)
/// }
/// </code>
/// </para>
/// <para>
/// This aspect uses <see cref="IntroduceDependencyAttribute"/> to inject
/// <see cref="IExceptionReporter"/> and <see cref="IVisualContextTracker"/> into the target type.
/// The current visual context (if any) is included in the exception report.
/// </para>
/// </remarks>
public class ReportExceptionsAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly IExceptionReporter _exceptionReporter;

    [IntroduceDependency]
    private readonly IVisualContextTracker _visualContextTracker;

    /// <summary>
    /// Gets or sets whether to swallow exceptions after reporting.
    /// Default is false (exceptions are rethrown).
    /// </summary>
    public bool Swallow { get; init; }

    /// <inheritdoc />
    public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
        base.BuildEligibility(builder);

        // Cannot be applied to abstract methods
        builder.MustNotBeAbstract();

        // Cannot be applied to extern methods
        builder.MustSatisfy(
            m => !m.IsExtern,
            m => $"{m} cannot be extern");
    }

    /// <inheritdoc />
    public override dynamic? OverrideMethod()
    {
        try
        {
            return meta.Proceed();
        }
        catch (Exception ex)
        {
            this._exceptionReporter.Report(ex, this._visualContextTracker.Current);

            if (this.Swallow)
            {
                return default;
            }

            throw;
        }
    }
}
