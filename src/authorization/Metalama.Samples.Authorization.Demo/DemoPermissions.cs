using Metalama.Framework.Aspects;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Permissions specific to the invoicing domain, in addition to <see cref="PermissionNames"/>.
/// They are <c>[RunTimeOrCompileTime]</c> because fabrics use them in options.
/// </summary>
[RunTimeOrCompileTime]
public static class DemoPermissions
{
    public const string CreateInvoice = nameof(CreateInvoice);
    public const string Approve = nameof(Approve);
    public const string Send = nameof(Send);
    public const string RecordPayment = nameof(RecordPayment);
    public const string Void = nameof(Void);
    public const string ReadSensitive = nameof(ReadSensitive);

    /// <summary>
    /// The permission to change the status of an invoice. No rule of the policy gives it to a role: the methods of the
    /// workflow, such as <c>Invoice.Approve</c>, grant it while they run, after their own permission has been checked.
    /// </summary>
    public const string ChangeStatus = nameof(ChangeStatus);
}
