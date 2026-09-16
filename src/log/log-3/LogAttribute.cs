using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        // Write entry message.
        var entryMessage = BuildInterpolatedString( false );
        entryMessage.AddText( " started." );
        Console.WriteLine( entryMessage.ToValue() );

        try
        {
            // Invoke the method and store the result in a variable.
            var result = meta.Proceed();

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

            Console.WriteLine( successMessage.ToValue() );

            return result;
        }
        catch ( Exception e )
        {
            // Display the failure message.
            var failureMessage = BuildInterpolatedString( false );
            failureMessage.AddText( " failed: " );
            failureMessage.AddExpression( e.Message );
            Console.WriteLine( failureMessage.ToValue() );

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