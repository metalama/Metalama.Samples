namespace Metalama.Samples.NormalizeStrings;

public class Class1
{
    // Applied on a property.
    [Trim]
    public string FirstName { get; set; } = "Borek";

    // Applied on a field.
    [Normalize]
    public string LastName = "Stavitel";

    // Applied on a parameter - nullable.
    public void SetCountry( [ToUpperCase] string? country )
    {
        Console.WriteLine( $"Country: {country}" );
    }
}