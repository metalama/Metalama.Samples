namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// A user of the invoicing application.
/// </summary>
public sealed record User( string Name ) : ISubject
{
    public override string ToString() => this.Name;
}
