using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder3;

[CompileTime]
internal class PropertyMappingFactory
{
    private static readonly HashSet<string> _immutableCollectionTypes =
    [
        nameof(ImmutableArray),
        nameof(ImmutableList),
        nameof(ImmutableDictionary),
        nameof(ImmutableQueue),
        nameof(ImmutableSortedSet),
        nameof(ImmutableStack)
    ];

    private readonly INamedType _sourceType;

    public PropertyMappingFactory( INamedType sourceType )
    {
        this._sourceType = sourceType;
    }

    public PropertyMapping Create( IProperty sourceProperty )
    {
        var isRequired = sourceProperty.Attributes.OfAttributeType( typeof(RequiredAttribute) ).Any();
        var isInherited = !sourceProperty.DeclaringType.Equals( this._sourceType );

        if ( IsImmutableCollectionType( sourceProperty.Type ) )
        {
            return new ImmutableCollectionPropertyMapping( sourceProperty, isRequired, isInherited );
        }
        else
        {
            return new StandardPropertyMapping( sourceProperty, isRequired, isInherited );
        }
    }

    private static bool IsImmutableCollectionType( IType type )
        => type is INamedType
           {
               ContainingNamespace.FullName: "System.Collections.Immutable"
           } namedType
           && _immutableCollectionTypes.Contains( namedType.Name );
}