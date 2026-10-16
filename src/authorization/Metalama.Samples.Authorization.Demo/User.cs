namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// A user account of the invoicing application. Users are identified by their unique <see cref="Name"/>.
/// </summary>
public sealed class User : ISubject
{
    // Used by Entity Framework.
    private User()
    {
        this.Name = null!;
        this.JobTitle = null!;
    }

    public User( string name, string jobTitle = "" )
    {
        this.Name = name;
        this.JobTitle = jobTitle;
    }

    public int Id { get; private set; }

    /// <summary>
    /// Gets the unique user name.
    /// </summary>
    public string Name { get; private set; }

    public string JobTitle { get; private set; }

    // Equality by name keeps role assignments and ownership comparable across database contexts,
    // which create different instances for the same user.
    public override bool Equals( object? obj ) => obj is User other && string.Equals( this.Name, other.Name, StringComparison.Ordinal );

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode( this.Name );

    public override string ToString() => this.Name;
}
