using System.ComponentModel.DataAnnotations;

#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.

namespace Metalama.Samples.Builder2.Tests.DerivedGenericType;

[GenerateBuilder]
public class StringKeyedValue<T>
{
    [Required]
    public T Value { get; }
}

public class TaggedKeyValue : StringKeyedValue<string>
{
    public string Tag { get; }
}