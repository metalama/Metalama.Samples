namespace Metalama.Samples.Authorization;

/// <summary>
/// Gives a securable object a label for exception messages and audit records, for instance <c>Invoice #12</c>.
/// </summary>
/// <remarks>
/// The label is shown to users who were denied access to the object, so it must not contain any data that the
/// object protects. Typically, it is made of the type name and the identifier. Objects that do not implement this
/// interface are labeled with their type name only. The framework reads the label in a system scope.
/// </remarks>
public interface IHasSecurityLabel
{
    string SecurityLabel { get; }
}

internal static class SecurityLabel
{
    public static string Of( object securable )
        => securable is IHasSecurityLabel labeled ? labeled.SecurityLabel : securable.GetType().Name;
}
