using Metalama.Patterns.Observability;

[Memento]
[Observable]
public partial class FishtankArtifact
{
    public string? Name { get; set; }

    public DateTime DateAdded { get; set; }
}