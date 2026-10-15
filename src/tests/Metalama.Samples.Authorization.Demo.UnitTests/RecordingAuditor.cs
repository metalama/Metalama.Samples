namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Collects denied accesses in memory.
/// </summary>
public sealed class RecordingAuditor : IAccessAuditor
{
    private readonly List<AccessDeniedException> _denials = [];

    public IReadOnlyList<AccessDeniedException> Denials => this._denials;

    public void OnAccessDenied( AccessDeniedException exception ) => this._denials.Add( exception );
}
