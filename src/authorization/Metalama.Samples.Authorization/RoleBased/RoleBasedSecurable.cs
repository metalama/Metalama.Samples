namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// Base class for objects secured by <see cref="RoleBasedAuthorizationPolicy"/>. It stores the owner and the role
/// assignments of the object, and resolves roles through the <see cref="SecurityParent"/> chain.
/// </summary>
public abstract class RoleBasedSecurable : IRoleBasedSecurable
{
    private readonly List<RoleAssignment> _roleAssignments = [];

    /// <summary>
    /// Gets or sets the owner of the object, who has the built-in <see cref="Role.Owner"/> role on it.
    /// </summary>
    [RequiresPermission( PermissionNames.Read, PermissionNames.ChangeOwnership )]
    public ISubject? Owner { get; set; }

    /// <summary>
    /// Gets the roles assigned on this object. Assignments inherited from the security parent are not included.
    /// </summary>
    [RequiresPermission( PermissionNames.Read )]
    public IReadOnlyList<RoleAssignment> RoleAssignments => this._roleAssignments;

    /// <summary>
    /// Gets the object from which this object inherits role assignments, or <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The security infrastructure reads this property, so it is excluded from default securing. Because options
    /// are inherited by overriding members, the exclusion also applies to overrides in derived classes.
    /// </remarks>
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    protected abstract IRoleBasedSecurable? SecurityParent { get; }

    IRoleBasedSecurable? IRoleBasedSecurable.SecurityParent => this.SecurityParent;

    /// <summary>
    /// Assigns a role to a subject on this object.
    /// </summary>
    [RequiresPermission( PermissionNames.ManageRoles )]
    public void AssignRole( ISubject subject, Role role )
    {
        if ( role.IsBuiltIn )
        {
            throw new ArgumentException( $"The built-in role '{role}' cannot be assigned.", nameof(role) );
        }

        var assignment = new RoleAssignment( subject, role );

        if ( !this._roleAssignments.Contains( assignment ) )
        {
            this._roleAssignments.Add( assignment );
        }
    }

    /// <summary>
    /// Revokes a role assigned to a subject on this object.
    /// </summary>
    /// <returns><c>true</c> if the assignment existed, otherwise <c>false</c>.</returns>
    [RequiresPermission( PermissionNames.ManageRoles )]
    public bool RevokeRole( ISubject subject, Role role ) => this._roleAssignments.Remove( new RoleAssignment( subject, role ) );

    /// <inheritdoc />
    public bool HasRole( ISubject subject, Role role )
    {
        if ( role == Role.Everyone )
        {
            return true;
        }

        // Ownership applies to the object itself and is not inherited by children.
        if ( role == Role.Owner )
        {
            return subject.Equals( this.Owner );
        }

        for ( IRoleBasedSecurable? ancestor = this; ancestor != null; ancestor = ancestor.SecurityParent )
        {
            if ( ancestor is RoleBasedSecurable securable
                 && securable._roleAssignments.Contains( new RoleAssignment( subject, role ) ) )
            {
                return true;
            }
        }

        return false;
    }
}
