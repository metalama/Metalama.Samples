using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.Extensions.Logging;

public class LogAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ILogger _logger;

    public override dynamic? OverrideMethod()
    {
        // Determine if tracing is enabled.
        var isTracingEnabled = this._logger.IsEnabled( LogLevel.Trace );

        // Write entry message.
        if ( isTracingEnabled )
        {
            var entryMessage = BuildInterpolatedString( false );
            entryMessage.AddText( " started." );

            using ( var guard = LoggingRecursionGuard.Begin() )
            {
                if ( guard.CanLog )
                {
                    this._logger.LogTrace( (string) entryMessage.ToValue() );
                }
            }
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

                    if ( SensitiveParameterFilter.IsSensitive(
                            meta.Target.Method
                                .ReturnParameter ) )
                    {
                        successMessage.AddText( "<redacted>" );
                    }
                    else
                    {
                        successMessage.AddExpression( result );
                    }

                    successMessage.AddText( "." );
                }

                using ( var guard = LoggingRecursionGuard.Begin() )
                {
                    if ( guard.CanLog )
                    {
                        this._logger.LogTrace( (string) successMessage.ToValue() );
                    }
                }
            }

            return result;
        }
        catch ( Exception e ) when ( this._logger.IsEnabled( LogLevel.Warning ) )
        {
            // Display the failure message.
            var failureMessage = BuildInterpolatedString( false );
            failureMessage.AddText( " failed: " );
            failureMessage.AddExpression( e.Message );

            using ( var guard = LoggingRecursionGuard.Begin() )
            {
                if ( guard.CanLog )
                {
                    this._logger.LogWarning( (string) failureMessage.ToValue() );
                }
            }

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

            if ( SensitiveParameterFilter.IsSensitive( p ) )
            {
                // Do not log sensitive parameters.
                stringBuilder.AddText( $"{comma}{p.Name} = <redacted> " );
            }
            else if ( p.RefKind == RefKind.Out && !includeOutParameters )
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