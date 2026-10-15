namespace Metalama.Samples.Authorization;

/// <summary>
/// Describes an access decision for auditing. It contains no reference to the securable object, only its label
/// (see <see cref="IHasSecurityLabel"/>), so that logging it cannot read secured data.
/// </summary>
/// <param name="Subject">The subject that requested the access.</param>
/// <param name="Permission">The required permission.</param>
/// <param name="Securable">The label of the object on which the permission is required.</param>
/// <param name="Member">The name of the member being accessed, for instance <c>Invoice.Approve</c>.</param>
/// <param name="AccessKind">The kind of access.</param>
public sealed record AccessRecord( ISubject Subject, Permission Permission, string Securable, string Member, AccessKind AccessKind );
