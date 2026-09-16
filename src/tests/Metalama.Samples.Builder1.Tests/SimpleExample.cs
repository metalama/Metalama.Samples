using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder1.Tests.SimpleExample;

[GenerateBuilder]
public partial class Song
{
    [Required]
    public string Artist { get; }

    [Required]
    public string Title { get; }

    public TimeSpan? Duration { get; }

    public string Genre { get; } = "General";
}