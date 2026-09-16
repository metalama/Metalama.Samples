namespace Metalama.Samples.Builder3.Tests.SimpleExample.GenericExample;
#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.
[GenerateBuilder]
public partial class StringKeyedValue<T>
{
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
    public Builder()
    {
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
