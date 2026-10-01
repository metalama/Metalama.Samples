using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;

public class CacheAttribute : OverrideMethodAspect
{
    // The ICache service is pulled from the dependency injection container. 
    // If needed, the aspect will add the field to the target class and pull it from
    // the constructor.
    [IntroduceDependency]
    private readonly ICache _cache;

    [IntroduceDependency]
    private readonly ICacheKeyBuilderProvider _cacheBuilderProvider;

    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        base.BuildAspect( builder );

        if ( !builder.Target.Compilation.IsPartial )
        {
            var cachingOptions = builder.Target.Enhancements().GetOptions<CachingOptions>();

            foreach ( var parameter in builder.Target.Parameters )
            {
                cachingOptions.VerifyCacheKeyMember( parameter, builder.Diagnostics );
            }
        }
    }

    public override dynamic? OverrideMethod()
    {
        if ( meta.Target.Compilation.IsPartial )
        {
            meta.InsertComment(
                "Design-time preview code may be different that compile-time code because of unresolved cache key builders." );
        }

        #region Build the caching key

        var stringBuilder = new InterpolatedStringBuilder();
        stringBuilder.AddText( meta.Target.Type.ToString() );
        stringBuilder.AddText( "." );
        stringBuilder.AddText( meta.Target.Method.Name );
        stringBuilder.AddText( "(" );

        var cachingOptions = meta.Target.Method.Enhancements().GetOptions<CachingOptions>();

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
            stringBuilder.AddText( ") " );
            stringBuilder.AddText( "{" );

            if ( cachingOptions.TryGetCacheKeyExpression(
                    p,
                    ExpressionFactory.Parse( nameof(this._cacheBuilderProvider) ),
                    out var cacheKeyExpression ) )
            {
                stringBuilder.AddExpression( cacheKeyExpression );
            }
            else
            {
                // This can happen at design time because we may have a partial compilation that
                // does not contain the aspects on other types.
                stringBuilder.AddText( $"<unresolved:{p.Name}>" );
            }

            stringBuilder.AddText( "}" );
        }

        stringBuilder.AddText( ")" );

        var cacheKey = (string) stringBuilder.ToValue();

        #endregion

        // Cache lookup.
        if ( this._cache.TryGetValue( cacheKey, out var value ) )
        {
            // Cache hit.
            return value;
        }

        // Cache miss. Go and invoke the method.
        var result = meta.Proceed();

        // Add to cache.
        this._cache.TryAdd( cacheKey, result );

        return result;
    }

    public override void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        // Do not allow or offer the aspect to be used on void methods or methods with out/ref parameters.

        builder.MustSatisfy(
            m => !m.ReturnType.Equals( SpecialType.Void ),
            m => $"{m} cannot be void" );

        builder.MustSatisfy(
            m => !m.Parameters.Any( p => p.RefKind is RefKind.Out or RefKind.Ref ),
            m => $"{m} cannot have out or ref parameter" );
    }
}