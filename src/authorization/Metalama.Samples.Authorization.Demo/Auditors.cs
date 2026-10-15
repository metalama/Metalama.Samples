namespace Metalama.Samples.Authorization.Demo;

/// <summary>
/// Writes every denied access to the console, as a stand-in for a security audit log.
/// </summary>
public sealed class ConsoleAuditor : IAccessAuditor
{
    public void OnAccessDenied( AccessDeniedException exception )
        => Console.WriteLine( $"        [audit] {exception.Subject.Name} was denied {exception.Permission} on {exception.Member}." );
}

/// <summary>
/// Collects denied accesses in memory, for tests.
/// </summary>
public sealed class RecordingAuditor : IAccessAuditor
{
    private readonly List<AccessDeniedException> _denials = [];

    public IReadOnlyList<AccessDeniedException> Denials => this._denials;

    public void OnAccessDenied( AccessDeniedException exception ) => this._denials.Add( exception );
}
