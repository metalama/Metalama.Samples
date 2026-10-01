namespace Metalama.Samples.Builder1.Tests.SimpleExample.GenericExample;
[GenerateBuilder]
public partial class StringKeyedValue<T>
{
  public T Value { get; }
  private StringKeyedValue(T value)
  {
    Value = value;
  }
  public Builder ToBuilder()
  {
    return new StringKeyedValue<T>.Builder(this);
  }
  public class Builder
  {
    public Builder()
    {
    }
    internal Builder(StringKeyedValue<T> source)
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
