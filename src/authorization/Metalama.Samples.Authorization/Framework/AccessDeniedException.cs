namespace Metalama.Samples.Authorization;

/// <summary>
/// Thrown when the current subject does not have the permission required by an operation.
/// </summary>
public sealed class AccessDeniedException : Exception
{
    public AccessDeniedException(
        ISubject subject,
        Permission permission,
        object securable,
        string member,
        AccessKind accessKind )
        : base( FormatMessage( subject, permission, securable, member, accessKind ) )
    {
        this.Subject = subject;
        this.Permission = permission;
        this.Securable = securable;
        this.Member = member;
        this.AccessKind = accessKind;
    }

    /// <summary>
    /// Gets the subject that was denied access.
    /// </summary>
    public ISubject Subject { get; }

    /// <summary>
    /// Gets the missing permission.
    /// </summary>
    public Permission Permission { get; }

    /// <summary>
    /// Gets the object on which the permission is missing. For a parameter check, this is the argument.
    /// </summary>
    public object Securable { get; }

    /// <summary>
    /// Gets the name of the member being accessed, for instance <c>Invoice.Amount</c>.
    /// </summary>
    public string Member { get; }

    /// <summary>
    /// Gets the kind of access that was denied.
    /// </summary>
    public AccessKind AccessKind { get; }

    private static string FormatMessage(
        ISubject subject,
        Permission permission,
        object securable,
        string member,
        AccessKind accessKind )
    {
        var verb = accessKind switch
        {
            AccessKind.Read => "read",
            AccessKind.Write => "write",
            _ => "execute"
        };

        return $"Cannot {verb} {member}: '{subject.Name}' does not have the {permission} permission on '{securable}'.";
    }
}
