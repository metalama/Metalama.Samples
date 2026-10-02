using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Eligibility;

public class CacheAttribute : OverrideMethodAspect
{
    [IntroduceDependency]
    private readonly ICache _cache;

    public override dynamic? OverrideMethod()
    {
        // Builds the caching string.
        var cacheKey = CacheKeyBuilder.GetCachingKey().ToValue();

        // Cache lookup.
        if ( this._cache.TryGetValue( cacheKey, out object value ) )
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
        builder.MustSatisfy(
            m => !m.ReturnType.Equals( SpecialType.Void ),
            m => $"{m} cannot be void" );

        builder.MustSatisfy(
            m => !m.Parameters.Any( p => p.RefKind is RefKind.Out or RefKind.Ref ),
            m => $"{m} cannot have out or ref parameter" );
    }
}