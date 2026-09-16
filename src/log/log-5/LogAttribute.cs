using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Microsoft.Extensions.Logging;

public class LogAttribute : MethodAspect
{
    private static readonly DiagnosticDefinition<INamedType> _missingLoggerFieldError =
        new(
            "LOG01",
            Severity.Error,
            "The type '{0}' must have a field 'ILogger _logger' or a property 'ILogger Logger'." );

    private static readonly DiagnosticDefinition<( DeclarationKind, IFieldOrProperty)>
        _loggerFieldOrIncorrectTypeError =
            new( "LOG02", Severity.Error, "The {0} '{1}' must be of type ILogger." );

    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        var declaringType = builder.Target.DeclaringType;

        // Finds a field named '_logger' or a property named 'Property'.
        var loggerFieldOrProperty =
            (IFieldOrProperty?) declaringType.AllFields.OfName( "_logger" ).SingleOrDefault() ??
            declaringType.AllProperties.OfName( "Logger" ).SingleOrDefault();

        // Report an error if the field or property does not exist.
        if ( loggerFieldOrProperty == null )
        {
            builder.Diagnostics.Report( _missingLoggerFieldError.WithArguments( declaringType ) );

            return;
        }

        // Verify the type of the logger field or property.
        if ( !loggerFieldOrProperty.Type.IsConvertibleTo( typeof(ILogger) ) )
        {
            builder.Diagnostics.Report(
                _loggerFieldOrIncorrectTypeError.WithArguments(
                    (declaringType.DeclarationKind,
                     loggerFieldOrProperty) ) );

            return;
        }

        // Override the target method with our template. Pass the logger field or property to the template.
        builder.Override(
            nameof(this.OverrideMethod),
            new { loggerFieldOrProperty } );
    }

    public override void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        base.BuildEligibility( builder );

        // Now that we reference an instance field, we cannot log static methods.
        builder.MustNotBeStatic();
    }

    [Template]
    private dynamic? OverrideMethod( IFieldOrProperty loggerFieldOrProperty )
    {
        // Define a `logger` run-time variable and assign it to the ILogger field or property,
        // e.g. `this._logger` or `this.Logger`.
        var logger = (ILogger) loggerFieldOrProperty.Value!;

        // Determine if tracing is enabled.
        var isTracingEnabled = logger.IsEnabled( LogLevel.Trace );

        // Write entry message.
        if ( isTracingEnabled )
        {
            var entryMessage = BuildInterpolatedString( false );
            entryMessage.AddText( " started." );
            LoggerExtensions.LogTrace( logger, entryMessage.ToValue() );
        }

        try
        {
            // Invoke the method and store the result in a variable.
            var result = meta.Proceed();

            if ( isTracingEnabled )
            {
                // Display the success message. The message is different when the method is void.
                var successMessage = BuildInterpolatedString( true );

                if ( meta.Target.Method.ReturnType.Equals( SpecialType.Void ) )
                {
                    // When the method is void, display a constant text.
                    successMessage.AddText( " succeeded." );
                }
                else
                {
                    // When the method has a return value, add it to the message.
                    successMessage.AddText( " returned " );
                    successMessage.AddExpression( result );
                    successMessage.AddText( "." );
                }

                LoggerExtensions.LogTrace( logger, successMessage.ToValue() );
            }

            return result;
        }
        catch ( Exception e ) when ( logger.IsEnabled( LogLevel.Warning ) )
        {
            // Display the failure message.
            var failureMessage = BuildInterpolatedString( false );
            failureMessage.AddText( " failed: " );
            failureMessage.AddExpression( e.Message );
            LoggerExtensions.LogWarning( logger, failureMessage.ToValue() );

            throw;
        }
    }

    // Builds an InterpolatedStringBuilder with the beginning of the message.
    private static InterpolatedStringBuilder BuildInterpolatedString( bool includeOutParameters )
    {
        var stringBuilder = new InterpolatedStringBuilder();

        // Include the type and method name.
        stringBuilder.AddText( meta.Target.Type.ToDisplayString( CodeDisplayFormat.MinimallyQualified ) );
        stringBuilder.AddText( "." );
        stringBuilder.AddText( meta.Target.Method.Name );
        stringBuilder.AddText( "(" );
        var i = 0;

        // Include a placeholder for each parameter.
        foreach ( var p in meta.Target.Parameters )
        {
            var comma = i > 0 ? ", " : "";

            if ( p.RefKind == RefKind.Out && !includeOutParameters )
            {
                // When the parameter is 'out', we cannot read the value.
                stringBuilder.AddText( $"{comma}{p.Name} = <out> " );
            }
            else
            {
                // Otherwise, add the parameter value.
                stringBuilder.AddText( $"{comma}{p.Name} = {{" );
                stringBuilder.AddExpression( p );
                stringBuilder.AddText( "}" );
            }

            i++;
        }

        stringBuilder.AddText( ")" );

        return stringBuilder;
    }
}