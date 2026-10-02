using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder2.Tests.DerivedType;

[GenerateBuilder]
public class Article
{
    [Required]
    public string Url { get; }

    [Required]
    public string Name { get; }
}

public class WebArticle : Article
{
    public string Keywords { get; }
}