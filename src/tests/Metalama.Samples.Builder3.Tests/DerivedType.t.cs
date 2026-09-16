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
  protected Article(string url, string name)
  {
    Url = url;
    Name = name;
  }
  public virtual Builder ToBuilder()
  {
    return new Builder(this);
  }
  public class Builder
  {
    public Builder(string url, string name)
    {
      Url = url;
      Name = name;
    }
    protected internal Builder(Article source)
    {
      Url = source.Url;
      Name = source.Name;
    }
    public string Name { get; set; }
    public string Url { get; set; }
    public Article Build()
    {
      var instance = new Article(Url, Name);
      return instance;
    }
  }
}
public class WebArticle : Article
{
  public string Keywords { get; }
  protected WebArticle(string keywords, string url, string name) : base(url, name)
  {
    Keywords = keywords;
  }
  public override Builder ToBuilder()
  {
    return new Builder(this);
  }
  public new class Builder : Article.Builder
  {
    public Builder(string url, string name) : base(url, name)
    {
    }
    protected internal Builder(WebArticle source) : base(source)
    {
      Keywords = source.Keywords;
    }
    public string Keywords { get; set; }
    public new WebArticle Build()
    {
      var instance = new WebArticle(Keywords, Url, Name);
      return instance;
    }
  }
}
