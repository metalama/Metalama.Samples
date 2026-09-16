using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

public class EnrichExceptionAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        // Compile-time code: create a formatting string containing the method name and placeholder for formatting parameters.
        var methodSignatureBuilder = new InterpolatedStringBuilder();
        methodSignatureBuilder.AddText( meta.Target.Type.ToString() );
        methodSignatureBuilder.AddText( "." );
        methodSignatureBuilder.AddText( meta.Target.Method.Name );
        methodSignatureBuilder.AddText( "(" );

        foreach ( var p in meta.Target.Parameters )
        {
            if ( p.Index > 0 )
            {
                methodSignatureBuilder.AddText( ", " );
            }

            if ( p.RefKind == RefKind.Out )
            {
                methodSignatureBuilder.AddText( $"{p.Name} = <out> " );
            }
            else
            {
                methodSignatureBuilder.AddExpression( p.Value );
            }
        }

        methodSignatureBuilder.AddText( ")" );

        try
        {
            return meta.Proceed();
        }
        catch ( Exception e )
        {
            e.AppendContextFrame( (string) methodSignatureBuilder.ToValue() );

            throw;
        }
    }
}