namespace Metalama.Samples.Authorization.Tests.Options_Disabled;

[ApplyDefaultPermissions]
public abstract class Entity
{
    public string? Name { get; set; }
}

// Default securing can be disabled for a type...
[AuthorizationOptions( IsDefaultSecuringEnabled = false )]
public class LookupValue : Entity
{
    public string? Code { get; set; }
}

public class Invoice : Entity
{
    // ...or for a single member.
    [AuthorizationOptions( IsDefaultSecuringEnabled = false )]
    public string? Number { get; set; }

    public decimal Amount { get; set; }
}
