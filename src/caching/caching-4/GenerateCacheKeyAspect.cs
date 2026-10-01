using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System.Text;

/// <summary>
///     Implements the <see cref="ICacheKey" /> interface based on <see cref="CacheKeyMemberAttribute" />
///     aspects on fields and properties. This aspect is implicitly added by <see cref="CacheKeyMemberAttribute" />
///     aspects.
///     It should never be added explicitly.
/// </summary>
[EditorExperience( SuggestAsAddAttribute = false )]
internal class GenerateCacheKeyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Implement the ICacheKey interface.        
        builder.ImplementInterface(
            typeof(ICacheKey),
            OverrideStrategy.Ignore );

        // Verify that all cache key members have a valid type.
        if ( !builder.Target.Compilation.IsPartial )
        {
            var cachingOptions = builder.Target.Enhancements().GetOptions<CachingOptions>();

            foreach ( var member in this.GetMembers( builder.Target ) )
            {
                cachingOptions.VerifyCacheKeyMember( member, builder.Diagnostics );
            }
        }
    }

    // Implementation of ICacheKey.ToCacheKey.
    [InterfaceMember]
    public string ToCacheKey( ICacheKeyBuilderProvider provider )
    {
        var stringBuilder = new StringBuilder();
        this.BuildCacheKey( stringBuilder, provider );

        return stringBuilder.ToString();
    }

    private IEnumerable<IFieldOrProperty> GetMembers( INamedType type )
        => type.FieldsAndProperties
            .Where( f => f.Enhancements().HasAspect<CacheKeyMemberAttribute>() )
            .OrderBy( f => f.Name );

    [Introduce( WhenExists = OverrideStrategy.Override )]
    protected virtual void BuildCacheKey(
        StringBuilder stringBuilder,
        ICacheKeyBuilderProvider provider )
    {
        // Call the base method, if any.
        if ( meta.Target.Method.IsOverride )
        {
            meta.Proceed();
            stringBuilder.Append( ", " );
        }

        if ( meta.Target.Compilation.IsPartial )
        {
            meta.InsertComment(
                "Design-time preview code may be different that compile-time code because of unresolved cache key builders." );
        }

        var cachingOptions = meta.Target.Type.Enhancements().GetOptions<CachingOptions>();

        // This is how we define a compile-time variable of value 0.
        var i = meta.CompileTime( 0 );

        foreach ( var member in this.GetMembers( meta.Target.Type ) )
        {
            if ( i > 0 )
            {
                stringBuilder.Append( ", " );
            }

            i++;

            if ( cachingOptions.TryGetCacheKeyExpression(
                    member,
                    ExpressionFactory.Parse( nameof(provider) ),
                    out var cacheKeyExpression ) )
            {
                stringBuilder.Append( cacheKeyExpression.Value );
            }
            else
            {
                // This can happen at design time because we may have a partial compilation that
                // does not contain the aspects on other types.
                stringBuilder.Append( $"<unresolved:{member.Name}>" );
            }
        }
    }
}