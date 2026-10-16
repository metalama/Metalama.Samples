namespace Metalama.Samples.Authorization;

/// <summary>
/// The kind of access being authorized. Each access has exactly one kind. The type is a set of flags so that a
/// filter, for instance in an <see cref="IAccessAuditor"/>, can select several kinds, such as <see cref="Change"/>.
/// </summary>
[Flags]
public enum AccessKind
{
    /// <summary>
    /// No access. It is the default value, and it selects nothing in a filter.
    /// </summary>
    None = 0,

    /// <summary>
    /// Executing a method, or passing an argument to a method.
    /// </summary>
    Execute = 1,

    /// <summary>
    /// Reading a field or property.
    /// </summary>
    Read = 2,

    /// <summary>
    /// Writing a field or property.
    /// </summary>
    Write = 4,

    /// <summary>
    /// Adding an item to a collection of the object, see <see cref="SecurableList{T}"/>.
    /// </summary>
    Add = 8,

    /// <summary>
    /// Removing items from a collection of the object, including clearing it.
    /// </summary>
    Remove = 16,

    /// <summary>
    /// Replacing an item of a collection of the object by another one.
    /// </summary>
    Replace = 32,

    /// <summary>
    /// Any change of the state of the object: writing a field or property, or changing one of its collections.
    /// </summary>
    Change = Write | Add | Remove | Replace
}
