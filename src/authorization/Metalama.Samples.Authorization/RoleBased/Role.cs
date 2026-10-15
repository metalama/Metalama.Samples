namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// A role. Roles are compared by name and kind.
/// </summary>
/// <remarks>
/// <para>An assigned role is given to subjects on an object with <see cref="RoleBasedSecurable.AssignRole"/>, and
/// inherited by the objects below it.</para>
/// <para>A computed role is never assigned: the object computes it from its own data, and children do not inherit
/// it. Two computed roles are built in: <see cref="Everyone"/>, which any subject has, and <see cref="Owner"/>, which
/// the owner of an object has. Applications can define more with <see cref="Computed"/>, and resolve them by
/// overriding <see cref="RoleBasedSecurable.HasComputedRole"/>.</para>
/// </remarks>
public sealed record Role
{
    private Role( string name, bool isComputed )
    {
        this.Name = name;
        this.IsComputed = isComputed;
    }

    /// <summary>
    /// Creates an assigned role.
    /// </summary>
    public Role( string name ) : this( name, false ) { }

    /// <summary>
    /// The built-in computed role that every subject has.
    /// </summary>
    public static Role Everyone { get; } = Computed( nameof(Everyone) );

    /// <summary>
    /// The built-in computed role that the <see cref="RoleBasedSecurable.Owner"/> of an object has on this object.
    /// </summary>
    public static Role Owner { get; } = Computed( nameof(Owner) );

    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the role is computed by the object instead of assigned.
    /// </summary>
    public bool IsComputed { get; }

    /// <summary>
    /// Creates a computed role.
    /// </summary>
    public static Role Computed( string name ) => new( name, true );

    public override string ToString() => this.Name;
}
