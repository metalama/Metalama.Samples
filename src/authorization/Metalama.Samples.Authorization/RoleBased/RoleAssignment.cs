namespace Metalama.Samples.Authorization.RoleBased;

/// <summary>
/// States that a subject has a role on an object and, through inheritance, on its descendants.
/// </summary>
public sealed record RoleAssignment( ISubject Subject, Role Role );
