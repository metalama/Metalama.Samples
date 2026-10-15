using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;

namespace Metalama.Samples.Authorization;

/// <summary>
/// How a secured property exposes a collection.
/// </summary>
[CompileTime]
internal enum CollectionKind
{
    /// <summary>
    /// The type is not a collection.
    /// </summary>
    None,

    /// <summary>
    /// <see cref="IList{T}"/> or <see cref="ICollection{T}"/>: the getter returns a <see cref="SecuredList{T}"/> view.
    /// </summary>
    Wrappable,

    /// <summary>
    /// A read-only type or interface. The class must not return a mutable object behind it.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// A mutable type that the view cannot replace, such as <see cref="List{T}"/>, an array, a set or a dictionary:
    /// any caller who can read the property can change its content.
    /// </summary>
    Mutable
}

[CompileTime]
internal static class CollectionTypes
{
    public static CollectionKind Classify( IType type )
    {
        if ( type is IArrayType )
        {
            return CollectionKind.Mutable;
        }

        if ( type is not INamedType namedType || namedType.SpecialType == SpecialType.String )
        {
            return CollectionKind.None;
        }

        var definition = namedType.Definition;

        if ( Is( definition, typeof(IList<>) ) || Is( definition, typeof(ICollection<>) ) )
        {
            return CollectionKind.Wrappable;
        }

        if ( Is( definition, typeof(IEnumerable<>) ) || Is( definition, typeof(IReadOnlyCollection<>) )
                                                    || Is( definition, typeof(IReadOnlyList<>) )
                                                    || Is( definition, typeof(IReadOnlyDictionary<,>) )
                                                    || Is( definition, typeof(IReadOnlySet<>) ) )
        {
            return CollectionKind.ReadOnly;
        }

        var isCollection = namedType.AllImplementedInterfaces.Any( i => Is( i.Definition, typeof(ICollection<>) ) )
                           || namedType.AllImplementedInterfaces.Any( i => Is( i, typeof(System.Collections.IList) ) );

        if ( !isCollection )
        {
            return CollectionKind.None;
        }

        var ns = namedType.ContainingNamespace.FullName;

        var isReadOnlyType = ns is "System.Collections.Immutable" or "System.Collections.Frozen"
                             || (ns == "System.Collections.ObjectModel" && namedType.Name.StartsWith( "ReadOnly", StringComparison.Ordinal ));

        return isReadOnlyType ? CollectionKind.ReadOnly : CollectionKind.Mutable;
    }

    private static bool Is( INamedType type, Type reflectionType ) => type.Equals( TypeFactory.GetType( reflectionType ) );
}
