namespace Metalama.Samples.Authorization.Demo.UnitTests;

/// <summary>
/// Collects denied accesses in memory.
/// </summary>
public sealed class RecordingAuditor : IAccessAuditor
{
    private readonly List<AccessDeniedException> _denials = [];

    private readonly List<AccessRecord> _grants = [];

    public IReadOnlyList<AccessDeniedException> Denials => this._denials;

    public IReadOnlyList<AccessRecord> Grants => this._grants;

    /// <summary>
    /// Gets the permissions whose grants are recorded.
    /// </summary>
    public HashSet<string> AuditedGrants { get; } = [];

    public void OnAccessDenied( AccessDeniedException exception ) => this._denials.Add( exception );

    public bool AuditsGrantsOf( Permission permission ) => this.AuditedGrants.Contains( permission.Name );

    public void OnAccessGranted( AccessRecord record ) => this._grants.Add( record );
}
