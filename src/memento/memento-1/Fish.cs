using Metalama.Patterns.Observability;

[Memento]
[Observable]
public partial class Fish
{
    public string? Name { get; set; }

    public string? Species { get; set; }

    public DateTime DateAdded { get; set; }
}