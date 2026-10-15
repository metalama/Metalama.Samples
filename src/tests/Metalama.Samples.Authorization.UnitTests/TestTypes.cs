using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.UnitTests;

// Small types independent of the demo domain, used to test the framework itself.

public sealed record TestUser( string Name ) : ISubject;

public class Folder : InMemoryRoleBasedSecurable
{
    public Folder? Parent { get; init; }

    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public string? Title { get; set; }

    protected override IRoleBasedSecurable? SecurityParent => this.Parent;

    public override string ToString() => "folder";
}

public class SpecialFolder : Folder;

public class NotSecurable;

public static class TestRoles
{
    public static Role Editor { get; } = new( nameof(Editor) );

    public static Role Reviewer { get; } = new( nameof(Reviewer) );
}

/// <summary>
/// A policy that grants everything and records the state of the security context while it runs.
/// </summary>
public sealed class ProbePolicy : IAuthorizationPolicy
{
    public bool WasSystemWhileEvaluating { get; private set; }

    public bool IsGranted( ISubject subject, Permission permission, object securable )
    {
        this.WasSystemWhileEvaluating = SecurityContext.IsSystem;

        // Reading a secured member from the policy must not recurse into another check.
        _ = ((Folder) securable).Title;

        return true;
    }
}
