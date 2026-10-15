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
}
