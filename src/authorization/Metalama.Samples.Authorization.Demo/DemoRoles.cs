using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Roles of the invoicing domain. They are assigned on a <see cref="Company"/> or a <see cref="Department"/>
/// and inherited by the objects below it.
/// </summary>
public static class DemoRoles
{
    /// <summary>
    /// Runs a department: approves its invoices, maintains its customers, sees its budget, staffs it.
    /// </summary>
    public static Role DepartmentManager { get; } = new( nameof(DepartmentManager) );

    /// <summary>
    /// Prepares and sends the invoices of a department, and maintains customer contact details.
    /// </summary>
    public static Role DepartmentAssistant { get; } = new( nameof(DepartmentAssistant) );

    /// <summary>
    /// Records payments, voids invoices and manages budgets, company-wide. Does not edit invoice content.
    /// </summary>
    public static Role Accountant { get; } = new( nameof(Accountant) );

    /// <summary>
    /// Reads everything, including sensitive data, and is denied any change (segregation of duties).
    /// </summary>
    public static Role Auditor { get; } = new( nameof(Auditor) );

    /// <summary>
    /// Manages role assignments everywhere, but is denied access to sensitive business data.
    /// </summary>
    public static Role Administrator { get; } = new( nameof(Administrator) );

    /// <summary>
    /// A computed role: the person who created an invoice has it on this invoice. Unlike the owner,
    /// the creator never changes.
    /// </summary>
    public static Role Creator { get; } = Role.Computed( nameof(Creator) );

    /// <summary>
    /// Gets the roles that the application defines, computed or assigned.
    /// </summary>
    public static IReadOnlyList<Role> All { get; } =
        [Role.Everyone, Role.Owner, Creator, DepartmentManager, DepartmentAssistant, Accountant, Auditor, Administrator];

    /// <summary>
    /// Gets the role that has a given name, as the database stores it, or <c>null</c> when the application does not
    /// define this role. A new role requires a change of this class, so that a mistyped name is an error instead of a
    /// role that nobody has.
    /// </summary>
    public static Role? FromName( string name ) => All.FirstOrDefault( role => role.Name == name );
}
