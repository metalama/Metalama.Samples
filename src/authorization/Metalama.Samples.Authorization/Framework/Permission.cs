namespace Metalama.Samples.Authorization;

/// <summary>
/// A permission, i.e. something that the current <see cref="ISubject"/> must be granted on an object
/// to read or write a field or property, or to execute a method. Permissions are compared by name.
/// </summary>
public sealed record Permission( string Name )
{
    public static implicit operator Permission( string name ) => new( name );

    public override string ToString() => this.Name;
}
