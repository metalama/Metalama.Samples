namespace Metalama.Samples.Comparison4;

public class DateComparer : IEqualityComparer<DateTime>
{
    public static DateComparer Instance { get; } = new();

    private DateComparer() { }

    public bool Equals( DateTime x, DateTime y )
    {
        return x.Date.Equals( y.Date );
    }

    public int GetHashCode( DateTime obj )
    {
        return obj.Date.GetHashCode();
    }
}