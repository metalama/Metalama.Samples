using Metalama.Samples.Authorization.Demo.Data;
using Metalama.Samples.Authorization.RoleBased;
using Microsoft.EntityFrameworkCore;

namespace Metalama.Samples.Authorization.Demo.Services;

/// <summary>
/// Application service for user accounts and role assignments.
/// </summary>
public sealed class StaffService( InvoicingDbContext db )
{
    /// <summary>
    /// Creates a user account. The operation requires <see cref="PermissionNames.ManageRoles"/> on the company.
    /// </summary>
    /// <remarks>
    /// A user is not a secured object, so this method checks the permission explicitly with
    /// <see cref="AccessGuard.Demand"/>, the same method that the aspects call.
    /// </remarks>
    public async Task<User> CreateUserAsync( string name, string jobTitle, CancellationToken cancellationToken = default )
    {
        var company = await db.Companies.SingleAsync( cancellationToken );
        AccessGuard.Demand( company, PermissionNames.ManageRoles, $"{nameof(StaffService)}.{nameof(this.CreateUserAsync)}", AccessKind.Execute );

        var user = new User( name, jobTitle );
        db.Users.Add( user );
        await db.SaveChangesAsync( cancellationToken );

        return user;
    }

    public async Task AssignDepartmentRoleAsync( string departmentCode, string userName, Role role, CancellationToken cancellationToken = default )
    {
        var department = await db.Departments.SingleAsync( d => d.Code == departmentCode, cancellationToken );
        var user = await db.Users.SingleAsync( u => u.Name == userName, cancellationToken );
        department.AssignRole( user, role );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task RevokeDepartmentRoleAsync( string departmentCode, string userName, Role role, CancellationToken cancellationToken = default )
    {
        var department = await db.Departments.SingleAsync( d => d.Code == departmentCode, cancellationToken );
        var user = await db.Users.SingleAsync( u => u.Name == userName, cancellationToken );
        department.RevokeRole( user, role );
        await db.SaveChangesAsync( cancellationToken );
    }

    public async Task AssignCompanyRoleAsync( string userName, Role role, CancellationToken cancellationToken = default )
    {
        var company = await db.Companies.SingleAsync( cancellationToken );
        var user = await db.Users.SingleAsync( u => u.Name == userName, cancellationToken );
        company.AssignRole( user, role );
        await db.SaveChangesAsync( cancellationToken );
    }
}
