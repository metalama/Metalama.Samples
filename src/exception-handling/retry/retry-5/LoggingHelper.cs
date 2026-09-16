using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Code;

[CompileTime]
internal static class LoggingHelper
{
    // Builds an InterpolatedStringBuilder with the beginning of the message.
    public static InterpolatedStringBuilder BuildInterpolatedString( bool includeOutParameters )
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