namespace Metalama.Samples.Builder1.Tests.SimpleExample.GenericExample;

[GenerateBuilder]
public partial class StringKeyedValue<T>
{
    public T Value { get; }
}