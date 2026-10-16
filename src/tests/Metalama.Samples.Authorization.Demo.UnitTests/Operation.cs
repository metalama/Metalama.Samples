namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// An operation of the demo application, with the set of actors that are allowed to perform it.
/// </summary>
/// <param name="Name">A unique, human-readable name. It appears in the test case names.</param>
/// <param name="AllowedActors">The actors for which the operation must succeed. It must be denied for all others.</param>
/// <param name="Act">The operation, executed as the actor on a fresh organization.</param>
/// <param name="Arrange">An optional preparation, executed in a system scope before <paramref name="Act"/>,
/// for instance to bring an invoice into the status that the operation requires.</param>
public sealed record Operation(
    string Name,
    IReadOnlySet<Actor> AllowedActors,
    Func<DemoOrganization, Task> Act,
    Func<DemoOrganization, Task>? Arrange = null )
{
    public override string ToString() => this.Name;
}
