using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;

namespace Metalama.Samples.Authorization;

/// <summary>
/// How the getter of a secured property returns a collection, so that a caller can never reach the mutable state
/// of the object.
/// </summary>
[CompileTime]
internal enum CollectionKind
{
    /// <summary>
    /// The type is not a collection: the getter returns the value.
    /// </summary>
    None,

    /// <summary>
    /// <see cref="IList{T}"/>, <see cref="ICollection{T}"/>, <see cref="IReadOnlyList{T}"/>,
    /// <see cref="IReadOnlyCollection{T}"/> or <see cref="IEnumerable{T}"/>: the getter sets the owner of the
    /// <see cref="GuardedList{T}"/> that the property returns, so that only authorized callers change it.
    /// </summary>
    Guarded,

    /// <summary>
    /// An immutable or read-only type, such as <c>ImmutableArray&lt;T&gt;</c>: the getter returns the value.
    /// </summary>
    Immutable,

    /// <summary>
    /// A type that the getter cannot guard: a mutable type, such as <see cref="List{T}"/>, an array, a set or a
    /// dictionary, or an interface of a dictionary or set, which may be backed by a mutable object.
    /// </summary>
    Unguarded
}

[CompileTime]
internal static class CollectionTypes
{
    public static CollectionKind Classify( IType type )
    {
        if ( type is IArrayType )
        {
            return CollectionKind.Unguarded;
        }

        if ( type is not INamedType namedType || namedType.SpecialType == SpecialType.String )
        {
            return CollectionKind.None;
        }

        var definition = namedType.Definition;

        // A property can also be declared with a guarded type, such as GuardedList<T>.
        if ( namedType.AllImplementedInterfaces.Any( i => Is( i, typeof(IGuardedCollection) ) ) )
        {
            return CollectionKind.Guarded;
        }

        if ( Is( definition, typeof(IList<>) ) || Is( definition, typeof(ICollection<>) )
                                              || Is( definition, typeof(IReadOnlyList<>) )
                                              || Is( definition, typeof(IReadOnlyCollection<>) )
                                              || Is( definition, typeof(IEnumerable<>) ) )
        {
            return CollectionKind.Guarded;
        }

        if ( Is( definition, typeof(IReadOnlyDictionary<,>) ) || Is( definition, typeof(IReadOnlySet<>) ) )
        {
            return CollectionKind.Unguarded;
        }

        var ns = namedType.ContainingNamespace.FullName;

        if ( ns is "System.Collections.Immutable" or "System.Collections.Frozen"
             || (ns == "System.Collections.ObjectModel" && namedType.Name.StartsWith( "ReadOnly", StringComparison.Ordinal )) )
        {
            return CollectionKind.Immutable;
        }

        var isCollection = namedType.AllImplementedInterfaces.Any( i => Is( i.Definition, typeof(ICollection<>) ) )
                           || namedType.AllImplementedInterfaces.Any( i => Is( i, typeof(System.Collections.IList) ) );

        return isCollection ? CollectionKind.Unguarded : CollectionKind.None;
    }

    private static bool Is( INamedType type, Type reflectionType ) => type.Equals( TypeFactory.GetType( reflectionType ) );
}
