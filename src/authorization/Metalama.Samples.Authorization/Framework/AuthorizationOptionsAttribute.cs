using Metalama.Framework.Options;

namespace Metalama.Samples.Authorization;

/// <summary>
/// Sets <see cref="AuthorizationOptions"/> on an assembly, a type or a member, as an alternative to a fabric.
/// </summary>
[AttributeUsage(
    AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct |
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method )]
public sealed class AuthorizationOptionsAttribute : Attribute, IHierarchicalOptionsProvider
{
    // Custom attributes cannot have properties of a nullable value type,
    // so nullable fields record whether the user set the property.
    private readonly bool? _secureNonPublicMembers;
    private readonly bool? _isDefaultSecuringEnabled;

    /// <inheritdoc cref="AuthorizationOptions.DefaultReadPermission"/>
    public string? DefaultReadPermission { get; init; }

    /// <inheritdoc cref="AuthorizationOptions.DefaultWritePermission"/>
    public string? DefaultWritePermission { get; init; }

    /// <inheritdoc cref="AuthorizationOptions.DefaultExecutePermission"/>
    public string? DefaultExecutePermission { get; init; }

    /// <inheritdoc cref="AuthorizationOptions.SecureNonPublicMembers"/>
    public bool SecureNonPublicMembers
    {
        get => this._secureNonPublicMembers ?? false;
        init => this._secureNonPublicMembers = value;
    }

    /// <inheritdoc cref="AuthorizationOptions.IsDefaultSecuringEnabled"/>
    public bool IsDefaultSecuringEnabled
    {
        get => this._isDefaultSecuringEnabled ?? true;
        init => this._isDefaultSecuringEnabled = value;
    }

    IEnumerable<IHierarchicalOptions> IHierarchicalOptionsProvider.GetOptions( in OptionsProviderContext context )
        =>
        [
            new AuthorizationOptions
            {
                DefaultReadPermission = this.DefaultReadPermission,
                DefaultWritePermission = this.DefaultWritePermission,
                DefaultExecutePermission = this.DefaultExecutePermission,
                SecureNonPublicMembers = this._secureNonPublicMembers,
                IsDefaultSecuringEnabled = this._isDefaultSecuringEnabled
            }
        ];
}
