namespace Metalama.Samples.Authorization;

/// <summary>
/// Marks the classes whose members are secured. Every class that implements this interface, directly or through
/// a derived interface such as <see cref="RoleBased.IRoleBasedSecurable"/>, inherits the
/// <see cref="SecurableTypeAttribute"/> aspect: its members get permission checks by default, and a validator
/// verifies that none is left unsecured.
/// </summary>
/// <remarks>
/// Being securable is part of the contract of the type, so it cannot be forgotten: an object that the policy can
/// secure is always secured and validated. A parameter with <see cref="RequiresPermissionAttribute"/> must also be
/// of a securable type, so that passing an object that no policy can secure is a build error.
/// </remarks>
[SecurableType]
public interface ISecurable;
