using Metalama.Samples.Authorization.Demo.Finance;
using Metalama.Samples.Authorization.Demo.Sales;
using Metalama.Samples.Authorization.RoleBased;
using static Metalama.Samples.Authorization.Demo.DemoPermissions;
using static Metalama.Samples.Authorization.Demo.DemoRoles;
using static Metalama.Samples.Authorization.PermissionNames;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// The role-permission rules of the invoicing domain. In a real application, they would be loaded from a database
/// and edited by an administrator, which is why they live in a run-time policy and not in build-time options.
/// </summary>
public static class DemoPolicy
{
    public static RoleBasedAuthorizationPolicy Create()
        => new RoleBasedAuthorizationPolicyBuilder()

            // The company directory is public.
            .Allow<Company>( Read, Role.Everyone )
            .Allow<Department>( Read, Role.Everyone )

            // The owner of an invoice can read and edit it. Neither the owner nor the creator can approve it:
            // the four-eyes principle. The creator rule matters because a manager can give an invoice to
            // another owner, but cannot change who created it.
            .Allow<Invoice>( Read, Role.Owner )
            .Allow<Invoice>( Write, Role.Owner )
            .Deny<Invoice>( Approve, Role.Owner )
            .Deny<Invoice>( Approve, Creator )

            // Department assistants prepare and send invoices, and maintain customer contact details.
            .Allow<Department>( CreateInvoice, DepartmentAssistant )
            .Allow<Invoice>( Read, DepartmentAssistant )
            .Allow<Invoice>( Write, DepartmentAssistant )
            .Allow<Invoice>( Send, DepartmentAssistant )
            .Allow<Customer>( Read, DepartmentAssistant )
            .Allow<Customer>( Write, DepartmentAssistant )

            // Department managers do everything assistants do, approve invoices and credit terms, see sensitive
            // customer data and the budget of their department, move invoices between departments they manage,
            // and staff their department.
            .Allow<object>( Read, DepartmentManager )
            .Allow<Department>( CreateInvoice, DepartmentManager )
            .Allow<Department>( ChangeOwnership, DepartmentManager )
            .Allow<Department>( ManageRoles, DepartmentManager )
            .Allow<Invoice>( Write, DepartmentManager )
            .Allow<Invoice>( Approve, DepartmentManager )
            .Allow<Invoice>( Send, DepartmentManager )
            .Allow<Invoice>( ChangeOwnership, DepartmentManager )
            .Allow<Customer>( Write, DepartmentManager )
            .Allow<Customer>( Approve, DepartmentManager )
            .Allow<Customer>( ChangeOwnership, DepartmentManager )
            .Allow<Customer>( ReadSensitive, DepartmentManager )
            .Allow<Budget>( ReadSensitive, DepartmentManager )

            // Accountants work company-wide on money, not on invoice content.
            .Allow<object>( Read, Accountant )
            .Allow<object>( ReadSensitive, Accountant )
            .Allow<Invoice>( RecordPayment, Accountant )
            .Allow<Invoice>( DemoPermissions.Void, Accountant )
            .Allow<Budget>( Write, Accountant )

            // Auditors see everything and change nothing. Deny wins over any role the auditor also has.
            .Allow<object>( Read, Auditor )
            .Allow<object>( ReadSensitive, Auditor )
            .Deny<object>( CreateInvoice, Auditor )
            .Deny<object>( Write, Auditor )
            .Deny<object>( Approve, Auditor )
            .Deny<object>( Send, Auditor )
            .Deny<object>( RecordPayment, Auditor )
            .Deny<object>( DemoPermissions.Void, Auditor )
            .Deny<object>( ChangeOwnership, Auditor )
            .Deny<object>( ManageRoles, Auditor )

            // Administrators manage roles but are kept away from sensitive data.
            .Allow<object>( ManageRoles, Administrator )
            .Deny<object>( ReadSensitive, Administrator )
            .Build();
}
