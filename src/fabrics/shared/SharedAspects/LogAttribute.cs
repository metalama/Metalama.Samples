using System.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Microsoft.Extensions.Logging;

namespace Metalama.Shared;

public class LogAttribute : MethodAspect
{
    private static readonly DiagnosticDefinition<INamedType> _missingLoggerFieldError =
        new("LOG01", Severity.Error,
            "The type '{0}' must have a field 'ILogger _logger' or a property 'ILogger Logger'.");

    /// <inheritdoc />
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var declaringType = builder.Target.DeclaringType;
        
        var loggerField =
            (IFieldOrProperty?)declaringType.AllFields.SingleOrDefault(field => field.Type.Is(typeof(ILogger))) ??
            declaringType.AllProperties.SingleOrDefault(prop => prop.Type.Is(typeof(ILogger)));

        if (loggerField == null)
        {
            builder.Diagnostics.Report(_missingLoggerFieldError.WithArguments(declaringType));
            return;
        }

        builder.Advice.Override(builder.Target, nameof(this.OverrideMethod), new { loggerFieldOrProperty = loggerField});
    }

    [Template]
    private dynamic? OverrideMethod(IFieldOrProperty loggerFieldOrProperty)
    {
        var logger = (ILogger) loggerFieldOrProperty.Value!;

        var entryMessage = BuildInterpolatedString();
        entryMessage.AddText(" started.");
        logger.LogTrace((string)entryMessage.ToValue());

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var result = meta.Proceed();

            var successMessage = BuildInterpolatedString();
            successMessage.AddText(" succeeded.");
            logger.LogTrace((string)successMessage.ToValue());

            return result;
        }
        catch (Exception ex)
        {
            var failureMessage = BuildInterpolatedString();
            failureMessage.AddText(" failed.");
            logger.LogError(ex, (string) failureMessage.ToValue());

            throw;
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMessage = BuildInterpolatedString();
            elapsedMessage.AddText(" elapsed: ");
            elapsedMessage.AddExpression(stopwatch.ElapsedMilliseconds);
            elapsedMessage.AddText("ms");
            logger.LogTrace((string)elapsedMessage.ToValue());
        }
    }

    private static InterpolatedStringBuilder BuildInterpolatedString()
    {
        var stringBuilder = new InterpolatedStringBuilder();

        stringBuilder.AddText(meta.Target.Type.ToDisplayString(CodeDisplayFormat.MinimallyQualified));
        stringBuilder.AddText(".");
        stringBuilder.AddText(meta.Target.Method.Name);

        return stringBuilder;
    }
}