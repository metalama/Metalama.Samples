using Metalama.Framework.Code;
using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization;

/// <summary>
/// Build-time options of the authorization aspects. They decide <i>which</i> checks are generated, while the
/// <see cref="IAuthorizationPolicy"/> decides at run time whether a check passes.
/// </summary>
/// <remarks>
/// <para>Options are inherited from the project, namespaces, base types, base members and declaring types. Set them
/// from a fabric with <c>amender.SetOptions(...)</c> or with the <see cref="AuthorizationOptionsAttribute"/>.</para>
/// <para>Every property is nullable: <c>null</c> means "inherit the value".</para>
/// <para>The defaults are applied by the aspects when a value is still <c>null</c>, and not by
/// <c>IHierarchicalOptions.GetDefaultOptions</c>. A base type contributes its fully merged options to derived
/// types, defaults included, and these override the namespace options of the derived type: a default
/// <c>Read</c> on <c>Entity</c> would cancel a <c>ReadSensitive</c> set on the namespace of a derived class.</para>
/// </remarks>
public sealed class AuthorizationOptions :
    IHierarchicalOptions<ICompilation>,
    IHierarchicalOptions<INamespace>,
    IHierarchicalOptions<INamedType>,
    IHierarchicalOptions<IFieldOrProperty>,
    IHierarchicalOptions<IMethod>,
    IHierarchicalOptions<IParameter>,
    IHierarchicalOptions<IIndexer>,
    IHierarchicalOptions<IEvent>
{
    /// <summary>
    /// Gets the permission required to read a field or property when <see cref="RequiresPermissionAttribute"/>
    /// does not name one. The default is <see cref="PermissionNames.Read"/>.
    /// </summary>
    public string? DefaultReadPermission { get; init; }

    /// <summary>
    /// Gets the permission required to write a field or property when <see cref="RequiresPermissionAttribute"/>
    /// does not name one. The default is <see cref="PermissionNames.Write"/>.
    /// </summary>
    public string? DefaultWritePermission { get; init; }

    /// <summary>
    /// Gets the permission required to execute a method when <see cref="RequiresPermissionAttribute"/> does not
    /// name one. The default is <see cref="PermissionNames.Write"/>, because a method typically changes the object.
    /// </summary>
    public string? DefaultExecutePermission { get; init; }

    /// <summary>
    /// Gets a value indicating whether <see cref="SecurableTypeAttribute"/> also secures
    /// <c>internal</c> and <c>protected</c> methods. Private methods are never secured by default. Fields and
    /// properties are always secured, whatever their accessibility, because they hold the state of the object.
    /// The default is <c>false</c>.
    /// </summary>
    public bool? SecureNonPublicMembers { get; init; }

    /// <summary>
    /// Gets a value indicating whether <see cref="SecurableTypeAttribute"/> secures the members it targets.
    /// Set it to <c>false</c> to exclude a namespace, a type or a member. The default is <c>true</c>.
    /// </summary>
    public bool? IsDefaultSecuringEnabled { get; init; }

    object IIncrementalObject.ApplyChanges( object changes, in ApplyChangesContext context )
    {
        var other = (AuthorizationOptions) changes;

        return new AuthorizationOptions
        {
            DefaultReadPermission = other.DefaultReadPermission ?? this.DefaultReadPermission,
            DefaultWritePermission = other.DefaultWritePermission ?? this.DefaultWritePermission,
            DefaultExecutePermission = other.DefaultExecutePermission ?? this.DefaultExecutePermission,
            SecureNonPublicMembers = other.SecureNonPublicMembers ?? this.SecureNonPublicMembers,
            IsDefaultSecuringEnabled = other.IsDefaultSecuringEnabled ?? this.IsDefaultSecuringEnabled
        };
    }
}
