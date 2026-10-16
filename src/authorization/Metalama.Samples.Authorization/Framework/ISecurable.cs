using Metalama.Framework.RunTime.Initialization;

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
public interface ISecurable
{
    /// <summary>
    /// Gets a value indicating whether the initialization of the object has ended. It is <c>false</c> from the first
    /// constructor until the end of the object initializer, or until <see cref="CompleteInitialization"/> when Metalama
    /// could not rewrite the call site. An object created in a system scope or without a security context is not
    /// tracked, so it is always initialized.
    /// </summary>
    bool IsInitialized => SecurityContext.IsInitialized( this );

    /// <summary>
    /// Ends the initialization of an object that was created where Metalama could not rewrite the call site, for
    /// instance by reflection or by a serializer. Until then, the getters and the methods of the object throw an
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <remarks>
    /// A call site that Metalama compiles calls <see cref="IInitializable.Initialize"/> after the object initializer,
    /// and does not need this method. The interface does not derive from <see cref="IInitializable"/>, because every
    /// class that implements it directly would then have to implement <see cref="IInitializable.Initialize"/> in its
    /// source code: Metalama introduces this method after the C# compiler has checked the source.
    /// </remarks>
    void CompleteInitialization()
    {
        if ( this is IInitializable initializable )
        {
            initializable.Initialize();
        }
        else
        {
            SecurityContext.EndInitialization( this );
        }
    }
}
