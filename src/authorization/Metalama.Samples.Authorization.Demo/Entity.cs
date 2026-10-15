using Metalama.Samples.Authorization.RoleBased;

namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Base class of all business objects. The <see cref="ApplyDefaultPermissionsAttribute"/> aspect is inherited,
/// so every derived class gets permission checks on its members, configured by <see cref="AuthorizationOptions"/>.
/// </summary>
[ApplyDefaultPermissions]
public abstract class Entity : RoleBasedSecurable;
