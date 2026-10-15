using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.UnitTests;

// Small types independent of the demo domain, used to test the framework itself.

public sealed record TestUser( string Name ) : ISubject;

public class Folder : InMemoryRoleBasedSecurable
{
    public IRoleBasedSecurable? Parent { get; init; }

    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public string? Title { get; set; }

    // The getter sets the owner of the securable list on the first access.
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public IList<string> Tags { get; } = new SecurableList<string>();

    // A read-only type is secured too: casting it back to IList<T> does not bypass the check.
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public IReadOnlyList<string> History { get; } = new SecurableList<string>();

    // A sequence that is not a collection is returned as it is.
    [RequiresPermission( PermissionNames.Read )]
    public IEnumerable<string> UpperTags => this.Tags.Select( t => t.ToUpperInvariant() );

    protected override IRoleBasedSecurable? SecurityParent => this.Parent;

    public override string ToString() => "folder";
}

public class SpecialFolder : Folder;

/// <summary>
/// A report whose members access other secured members, to show that permissions are not transitive.
/// </summary>
public sealed class Report : InMemoryRoleBasedSecurable
{
    private string? _draftNotes;

    // A detail.
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public string? Body { get; set; }

    // An aggregate computed from a secured detail: reading it also reads Body, which is checked.
    [RequiresPermission( "Summarize" )]
    public int BodyLength => this.Body?.Length ?? 0;

    // An aggregate computed from private state, which is never secured.
    [RequiresPermission( "Summarize" )]
    public int DraftNotesLength => this._draftNotes?.Length ?? 0;

    // A method that reads and writes a secured property: each access is checked.
    [RequiresPermission( "Publish" )]
    public void Publish() => this.Body = this.Body + " (published)";

    public void SetDraftNotes( string notes ) => this._draftNotes = notes;

    // An aggregate that grants Read on the report while it runs: the subject needs Summarize only.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( PermissionNames.Read )]
    public int GrantedBodyLength => this.Body?.Length ?? 0;

    // The grant covers the continuations of an async member.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( PermissionNames.Read )]
    public async Task<int> GetBodyLengthAsync()
    {
        await Task.Yield();

        return this.Body?.Length ?? 0;
    }

    // The grant applies to this report, not to the other one.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( PermissionNames.Read )]
    public int CompareLength( Report other ) => (this.Body?.Length ?? 0) - (other.Body?.Length ?? 0);

    // The grant applies to the current context, not to another one.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( PermissionNames.Read )]
    public string? ReadBodyAs( ISecurityContext context )
    {
        using ( SecurityContext.Use( context ) )
        {
            return this.Body;
        }
    }

    // A grant cannot satisfy the check of its own member.
    [RequiresPermission( "Summarize" )]
    [GrantsPermissions( "Summarize" )]
    public int SelfGranted => 0;

    // A method that grants both permissions of the property it reads and writes.
    [RequiresPermission( "Publish" )]
    [GrantsPermissions( PermissionNames.Read, PermissionNames.Write )]
    public void PublishWithGrant() => this.Body = this.Body + " (published)";

    protected override IRoleBasedSecurable? SecurityParent => null;
}

/// <summary>
/// A folder that returns an ordinary list from a secured property: the getter fails closed.
/// </summary>
public sealed class MutableListFolder : InMemoryRoleBasedSecurable
{
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public IList<string> Tags { get; } = new List<string>();

    protected override IRoleBasedSecurable? SecurityParent => null;
}

/// <summary>
/// A folder that refuses changes of its tags once it is archived, through the owner hook.
/// </summary>
public sealed class ArchivableFolder : InMemoryRoleBasedSecurable, ISecurableCollectionOwner
{
    [RequiresPermission( PermissionNames.Read, PermissionNames.Write )]
    public IList<string> Tags { get; } = new SecurableList<string>();

    public bool IsArchived { get; set; }

    public List<string> ChangedProperties { get; } = [];

    public List<AccessKind> ChangeKinds { get; } = [];

    protected override IRoleBasedSecurable? SecurityParent => null;

    void ISecurableCollectionOwner.OnCollectionChanging( string propertyName, AccessKind accessKind )
    {
        this.ChangedProperties.Add( propertyName );
        this.ChangeKinds.Add( accessKind );

        if ( this.IsArchived )
        {
            throw new InvalidOperationException( "The folder is archived." );
        }
    }
}

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

// A securable object that the role-based policy does not know how to secure.
public class NotRoleBased : ISecurable;

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

    public bool IsGranted( ISubject subject, Permission permission, ISecurable securable )
    {
        this.WasSystemWhileEvaluating = SecurityContext.IsSystem;

        // Reading a secured member from the policy must not recurse into another check.
        _ = ((Folder) securable).Title;

        return true;
    }
}
