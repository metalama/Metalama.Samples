using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

[CompileTime]
internal static class CacheKeyBuilder
{
    public static InterpolatedStringBuilder GetCachingKey()
    {
        var stringBuilder = new InterpolatedStringBuilder();
        stringBuilder.AddText( meta.Target.Type.ToString() );
        stringBuilder.AddText( "." );
        stringBuilder.AddText( meta.Target.Method.Name );
        stringBuilder.AddText( "(" );

        foreach ( var p in meta.Target.Parameters )
        {
            if ( p.Index > 0 )
            {
                stringBuilder.AddText( ", " );
            }

            // We have to add the parameter type to avoid ambiguities
            // between different overloads of the same method.
            stringBuilder.AddText( "(" );
            stringBuilder.AddText( p.Type.ToString() );
            stringBuilder.AddText( ")" );

            stringBuilder.AddText( "{" );
            stringBuilder.AddExpression( p );
            stringBuilder.AddText( "}" );
        }

        stringBuilder.AddText( ")" );

        return stringBuilder;
    }
}