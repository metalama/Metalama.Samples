using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.UnitTests;

// Small types independent of the demo domain, used to test the framework itself.

public sealed record TestUser( string Name ) : ISubject;

public class Folder : InMemoryRoleBasedSecurable
{
    public IRoleBasedSecurable? Parent { get; init; }

    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public string? Title { get; set; }

    // The getter returns a secured view of the list.
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public IList<string> Tags { get; } = new List<string>();

    protected override IRoleBasedSecurable? SecurityParent => this.Parent;

    public override string ToString() => "folder";
}

public class SpecialFolder : Folder;

/// <summary>
/// A folder with a label and a name that must not appear in messages.
/// </summary>
public sealed class LabeledFolder : Folder, IHasSecurityLabel
{
    public string? Name { get; set; }

    public string SecurityLabel => "Folder #7";

    public override string ToString() => this.Name ?? "";
}

/// <summary>
/// A folder whose reviewer is computed from its own data, like the creator of an invoice.
/// </summary>
public sealed class ReviewedFolder : Folder
{
    public ISubject? Reviewer { get; init; }

    protected override bool HasComputedRole( ISubject subject, Role role ) => role == TestRoles.Reviewer && subject.Equals( this.Reviewer );
}

/// <summary>
/// A securable that implements the interface without deriving from <see cref="RoleBasedSecurable"/>,
/// for instance an object of another library.
/// </summary>
public sealed class ExternalSecurable( ISubject editor ) : IRoleBasedSecurable
{
    public IRoleBasedSecurable? SecurityParent => null;

    public bool HasRole( ISubject subject, Role role ) => role == TestRoles.Editor && subject.Equals( editor );
}

public interface IFolder;

public class NotSecurable;

public static class TestRoles
{
    public static Role Editor { get; } = new( nameof(Editor) );

    public static Role Auditor { get; } = new( nameof(Auditor) );

    public static Role Reviewer { get; } = Role.Computed( nameof(Reviewer) );
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
