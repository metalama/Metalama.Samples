using System.ComponentModel.DataAnnotations;

namespace Metalama.Samples.Builder3.Tests.DerivedType;

#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.

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