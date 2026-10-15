namespace Metalama.Samples.Authorization.Tests.Options_TypeAttribute;

[ApplyDefaultPermissions]
[AuthorizationOptions( DefaultWritePermission = "Approve" )]
public class Customer
{
    // Write requires Approve, from the type-level option.
    public decimal CreditLimit { get; set; }

    // The member-level option wins over the type-level option.
    [AuthorizationOptions( DefaultWritePermission = "Write" )]
    public string? Email { get; set; }

    // An explicit attribute wins over all options.
    [RequiresPermission( "ReadSensitive", "Write" )]
    public string? TaxId { get; set; }
}
