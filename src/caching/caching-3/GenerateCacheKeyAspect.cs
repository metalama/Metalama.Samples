using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Text;

/// <summary>
/// Implements the <see cref="ICacheKey"/> interface based on <see cref="CacheKeyMemberAttribute"/> 
/// aspects on fields and properties. This aspect is implicitly added by <see cref="CacheKeyMemberAttribute"/> aspects.
/// It should never be added explicitly.
/// </summary>
[EditorExperience( SuggestAsAddAttribute = false )]
internal class GenerateCacheKeyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
        => builder.ImplementInterface(
            typeof(ICacheKey),
            OverrideStrategy.Ignore );

    // Implementation of ICacheKey.ToCacheKey.
    [InterfaceMember]
    public string ToCacheKey()
    {
        var stringBuilder = new StringBuilder();
        this.BuildCacheKey( stringBuilder );

        return stringBuilder.ToString();
    }

    [Introduce( WhenExists = OverrideStrategy.Override )]
    protected virtual void BuildCacheKey( StringBuilder stringBuilder )
    {
        // Call the base method, if any.
        if ( meta.Target.Method.IsOverride )
        {
            meta.Proceed();
            stringBuilder.Append( ", " );
        }

        // Select all cache key members.
        var members =
            meta.Target.Type.FieldsAndProperties
                .Where( f => f.Enhancements().HasAspect<CacheKeyMemberAttribute>() )
                .OrderBy( f => f.Name )
                .ToList();

        // This is how we define a compile-time variable of value 0.

        var i = meta.CompileTime( 0 );

        foreach ( var member in members )
        {
            if ( i > 0 )
            {
                stringBuilder.Append( ", " );
            }

            i++;

            // Check if the parameter type implements ICacheKey or has an aspect of type GenerateCacheKeyAspect.
            if ( member.Type.IsConvertibleTo( typeof(ICacheKey) ) ||
                 (member.Type is INamedType { BelongsToCurrentProject: true } namedType &&
                  namedType.Enhancements().HasAspect<GenerateCacheKeyAspect>()) )
            {
                // If the parameter is ICacheKey, use it.
                if ( member.Type.IsNullable == false )
                {
                    stringBuilder.Append( member.Value!.ToCacheKey() );
                }
                else
                {
                    stringBuilder.Append( member.Value?.ToCacheKey() ?? "null" );
                }
            }
            else
            {
                if ( member.Type.IsNullable == false )
                {
                    stringBuilder.Append( member.Value );
                }
                else
                {
                    stringBuilder.Append( member.Value?.ToString() ?? "null" );
                }
            }
        }
    }
}