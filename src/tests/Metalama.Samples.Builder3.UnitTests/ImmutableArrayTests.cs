using System.Collections.Immutable;
using Xunit;

namespace Metalama.Samples.Builder3.UnitTests;

public partial class ImmutableArrayTests
{
    [Fact]
    public void NoItemAdded_IsEmpty()
    {
        var builder = new ColorWheel.Builder();
        var immutable = builder.Build();

        Assert.False( immutable.Colors.IsDefault );
        Assert.True( immutable.Colors.IsEmpty );
    }

    [Fact]
    public void ItemAdded_NotEmpty()
    {
        var builder = new ColorWheel.Builder();
        builder.Colors.Add( "red" );
        var immutable = builder.Build();

        Assert.Single( immutable.Colors );
        Assert.Equal( "red", immutable.Colors[0] );
    }

    [Fact]
    public void BuildBuilderFromItem()
    {
        var builder1 = new ColorWheel.Builder();
        builder1.Colors.Add( "red" );
        var immutable1 = builder1.Build();

        var builder2 = immutable1.ToBuilder();
        builder2.Colors.Add( "blue" );
        var immutable2 = builder2.Build();

        Assert.Contains( "red", immutable2.Colors );
        Assert.Contains( "blue", immutable2.Colors );
    }

    [GenerateBuilder]
    public partial class ColorWheel
    {
        public ImmutableArray<string> Colors { get; }
    }
}