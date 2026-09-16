namespace Metalama.Samples.Comparison4;

public class TrimmingStringEqualityComparer : IEqualityComparer<string>
{
    public static TrimmingStringEqualityComparer Ordinal { get; } = new( StringComparer.Ordinal );

    public static TrimmingStringEqualityComparer OrdinalIgnoreCase { get; } = new( StringComparer.OrdinalIgnoreCase );

    public static TrimmingStringEqualityComparer InvariantCulture { get; } = new( StringComparer.InvariantCulture );

    public static TrimmingStringEqualityComparer InvariantCultureIgnoreCase { get; } =
        new( StringComparer.InvariantCultureIgnoreCase );

    public static TrimmingStringEqualityComparer CurrentCulture { get; } = new( StringComparer.CurrentCulture );

    public static TrimmingStringEqualityComparer CurrentCultureIgnoreCase { get; } =
        new( StringComparer.CurrentCultureIgnoreCase );

    private readonly StringComparer _underlying;

    private TrimmingStringEqualityComparer( StringComparer underlying )
    {
        this._underlying = underlying;
    }

    // Note that a faster implementation would not call Trim but use ReadOnlySpan<char>,
    // but there is no equivalent of StringComparer for ReadOnlySpan<char>.
    public bool Equals( string? x, string? y ) => this._underlying.Equals( x?.Trim(), y?.Trim() );

    public int GetHashCode( string obj ) => this._underlying.GetHashCode( obj.Trim() );
}