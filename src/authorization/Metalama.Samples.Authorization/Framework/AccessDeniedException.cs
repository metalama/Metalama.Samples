namespace Metalama.Samples.Authorization;

/// <summary>
/// Thrown when the current subject is not allowed to perform an operation.
/// </summary>
/// <remarks>
/// The exception does not keep a reference to the securable object, and its message names the object only by its
/// label (see <see cref="IHasSecurityLabel"/>). A user who is denied access therefore learns nothing about the
/// object, and a logger that serializes the exception cannot read secured members.
/// </remarks>
public sealed class AccessDeniedException : Exception
{
    public AccessDeniedException( AccessRecord record, string? reason = null )
        : base( FormatMessage( record, reason ) )
    {
        this.Record = record;
    }

    /// <summary>
    /// Gets the description of the denied access.
    /// </summary>
    public AccessRecord Record { get; }

    /// <summary>
    /// Gets the subject that was denied access.
    /// </summary>
    public ISubject Subject => this.Record.Subject;

    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public Permission Permission => this.Record.Permission;

    /// <summary>
    /// Gets the label of the object on which the permission is required. For a parameter check, it is the label
    /// of the argument.
    /// </summary>
    public string Securable => this.Record.Securable;

    /// <summary>
    /// Gets the name of the member being accessed, for instance <c>Invoice.Amount</c>.
    /// </summary>
    public string Member => this.Record.Member;

    /// <summary>
    /// Gets the kind of access that was denied.
    /// </summary>
    public AccessKind AccessKind => this.Record.AccessKind;

    private static string FormatMessage( AccessRecord record, string? reason )
    {
        var verb = record.AccessKind switch
        {
            AccessKind.Read => "read",
            AccessKind.Write => "write",
            _ => "execute"
        };

        reason ??= $"'{record.Subject.Name}' does not have the {record.Permission} permission on {record.Securable}";

        return $"Cannot {verb} {record.Member}: {reason}.";
    }
}
