using Metalama.Framework.Aspects;

namespace Metalama.Samples.Authorization;

/// <summary>
/// Names of the permissions that the framework itself relies on. Applications add their own.
/// These are constants so that they can be used as custom attribute arguments, and they are
/// <c>[RunTimeOrCompileTime]</c> so that aspects can use them as defaults.
/// </summary>
[RunTimeOrCompileTime]
public static class PermissionNames
{
    public const string Read = nameof(Read);
    public const string Write = nameof(Write);
    public const string ChangeOwnership = nameof(ChangeOwnership);
    public const string ManageRoles = nameof(ManageRoles);
}
