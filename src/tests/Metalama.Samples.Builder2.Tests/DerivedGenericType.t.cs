using System.ComponentModel.DataAnnotations;
#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.
namespace Metalama.Samples.Builder2.Tests.DerivedGenericType;
[GenerateBuilder]
public class StringKeyedValue<T>
{
  [Required]
  public T Value { get; }
  protected StringKeyedValue(T value)
  {
    Value = value;
  }
  public virtual Builder ToBuilder()
  {
    return new StringKeyedValue<T>.Builder(this);
  }
  public class Builder
  {
    public Builder(T value)
    {
      Value = value;
    }
    protected internal Builder(StringKeyedValue<T> source)
    {
      Value = source.Value;
    }
    public T Value { get; set; }
    public StringKeyedValue<T> Build()
    {
      var instance = new StringKeyedValue<T>(Value);
      return instance;
    }
  }
}
public class TaggedKeyValue : StringKeyedValue<string>
{
  public string Tag { get; }
  protected TaggedKeyValue(string tag, string value) : base(value)
  {
    Tag = tag;
  }
  public override Builder ToBuilder()
  {
    return new Builder(this);
  }
  public new class Builder : StringKeyedValue<string>.Builder
  {
    public Builder(string value) : base(value)
    {
    }
    protected internal Builder(TaggedKeyValue source) : base(source)
    {
      Tag = source.Tag;
    }
    public string Tag { get; set; }
    public new TaggedKeyValue Build()
    {
      var instance = new TaggedKeyValue(Tag, Value);
      return instance;
    }
  }
}
