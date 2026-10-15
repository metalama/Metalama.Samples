namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// Base class for objects secured by <see cref="RoleBasedAuthorizationPolicy"/>. It implements role resolution:
/// computed roles, assigned roles, and inheritance through the <see cref="SecurityParent"/> chain.
/// </summary>
/// <remarks>
/// <para>The class does not store anything. Derived classes store the owner and the role assignments, for instance
/// in memory or in a database mapped with Entity Framework.</para>
/// <para>The members that the security infrastructure calls are excluded from default securing. Because options are
/// inherited by overriding members, the exclusion also applies to the overrides in derived classes.</para>
/// </remarks>
public abstract class RoleBasedSecurable : IRoleBasedSecurable
{
    /// <summary>
    /// Gets or sets the owner of the object, who has the built-in <see cref="Role.Owner"/> role on it.
    /// </summary>
    /// <remarks>
    /// The aspect is inherited, so the overrides in derived classes are secured.
    /// </remarks>
    [RequiresPermission( PermissionNames.Read, PermissionNames.ChangeOwnership )]
    public abstract ISubject? Owner { get; set; }

    /// <summary>
    /// Gets the roles assigned on this object. Assignments inherited from the security parent are not included.
    /// </summary>
    [RequiresPermission( PermissionNames.Read )]
    public IReadOnlyList<RoleAssignment> RoleAssignments => [.. this.GetRoleAssignments()];

    /// <summary>
    /// Gets the object from which this object inherits assigned roles, or <c>null</c>.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract IRoleBasedSecurable? SecurityParent { get; }

    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    IRoleBasedSecurable? IRoleBasedSecurable.SecurityParent => this.SecurityParent;

    /// <summary>
    /// Returns the role assignments stored on this object.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract IEnumerable<RoleAssignment> GetRoleAssignments();

    /// <summary>
    /// Stores a new role assignment on this object. The assignment is not stored yet.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract void AddRoleAssignment( RoleAssignment assignment );

    /// <summary>
    /// Removes a role assignment stored on this object.
    /// </summary>
    /// <returns><c>true</c> if the assignment existed, otherwise <c>false</c>.</returns>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract bool RemoveRoleAssignment( RoleAssignment assignment );

    /// <summary>
    /// Determines whether <paramref name="subject"/> has a computed role defined by the application
    /// (see <see cref="Role.Computed"/>). The default implementation returns <c>false</c>.
    /// </summary>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected virtual bool HasComputedRole( ISubject subject, Role role ) => false;

    /// <summary>
    /// Assigns a role to a subject on this object. Nobody can assign a role to themself.
    /// </summary>
    [RequiresPermission( PermissionNames.ManageRoles )]
    public void AssignRole( ISubject subject, Role role )
    {
        if ( role.IsComputed )
        {
            throw new ArgumentException( $"The computed role '{role}' cannot be assigned.", nameof(role) );
        }

        // Without this rule, anyone who manages roles could give themself any right.
        if ( !SecurityContext.IsSystem && subject.Equals( SecurityContext.Current?.Subject ) )
        {
            AccessGuard.Deny(
                this,
                PermissionNames.ManageRoles,
                $"{nameof(RoleBasedSecurable)}.{nameof(this.AssignRole)}",
                $"'{subject.Name}' cannot assign a role to themself" );
        }

        var assignment = new RoleAssignment( subject, role );

        if ( !this.GetRoleAssignments().Contains( assignment ) )
        {
            this.AddRoleAssignment( assignment );
        }
    }

    /// <summary>
    /// Revokes a role assigned to a subject on this object.
    /// </summary>
    /// <returns><c>true</c> if the assignment existed, otherwise <c>false</c>.</returns>
    [RequiresPermission( PermissionNames.ManageRoles )]
    public bool RevokeRole( ISubject subject, Role role ) => this.RemoveRoleAssignment( new RoleAssignment( subject, role ) );

    /// <inheritdoc />
    /// <remarks>The policy calls this method in a system scope, so it is excluded from default securing.</remarks>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public bool HasRole( ISubject subject, Role role )
    {
        // Computed roles apply to the object itself and are not inherited by children.
        if ( role.IsComputed )
        {
            if ( role == Role.Everyone )
            {
                return true;
            }

            if ( role == Role.Owner )
            {
                return subject.Equals( this.Owner );
            }

            return this.HasComputedRole( subject, role );
        }

        if ( this.GetRoleAssignments().Contains( new RoleAssignment( subject, role ) ) )
        {
            return true;
        }

        // Delegating to the parent supports any implementation of IRoleBasedSecurable in the chain.
        return this.SecurityParent?.HasRole( subject, role ) == true;
    }
}
