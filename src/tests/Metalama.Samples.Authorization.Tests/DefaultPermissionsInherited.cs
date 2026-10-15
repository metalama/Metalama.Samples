namespace Metalama.Samples.Authorization.Tests.DefaultPermissionsInherited;

[SecurableType]
public abstract class Entity
{
    public string? Name { get; set; }
}

// The aspect is [Inheritable], so the derived class gets it without an attribute.
public class Invoice : Entity
{
    public decimal Amount { get; set; }
}
