namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// A role. Roles are compared by name.
/// </summary>
/// <remarks>
/// Two roles are built in and never assigned explicitly: <see cref="Everyone"/>, which any subject has,
/// and <see cref="Owner"/>, which the owner of an object has on that object. All other roles are assigned
/// to subjects on an object with <see cref="RoleBasedSecurable.AssignRole"/>.
/// </remarks>
public sealed record Role( string Name )
{
    /// <summary>
    /// The built-in role that every subject has.
    /// </summary>
    public static Role Everyone { get; } = new( nameof(Everyone) );

    /// <summary>
    /// The built-in role that the <see cref="RoleBasedSecurable.Owner"/> of an object has on this object.
    /// </summary>
    public static Role Owner { get; } = new( nameof(Owner) );

    /// <summary>
    /// Gets a value indicating whether the role is built in, i.e. computed instead of assigned.
    /// </summary>
    public bool IsBuiltIn => this == Everyone || this == Owner;

    public override string ToString() => this.Name;
}
