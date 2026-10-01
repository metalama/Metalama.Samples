using System.Collections.Immutable;
namespace Metalama.Samples.Builder3.Tests.SimpleExample._ImmutableArray;
#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.
[GenerateBuilder]
public partial class ColorWheel
{
  public ImmutableArray<string> Colors { get; }
  protected ColorWheel(ImmutableArray<string> colors)
  {
    Colors = colors;
  }
  public virtual Builder ToBuilder()
  {
    return new Builder(this);
  }
  public class Builder
  {
    private ImmutableArray<string> _colors = ImmutableArray<string>.Empty;
    private ImmutableArray<string>.Builder? _colorsBuilder;
    public Builder()
    {
    }
    protected internal Builder(ColorWheel source)
    {
      _colors = source.Colors;
    }
    public ImmutableArray<string>.Builder Colors
    {
      get
      {
        return _colorsBuilder ??= _colors.ToBuilder();
      }
    }
    public ColorWheel Build()
    {
      var instance = new ColorWheel(GetImmutableColors());
      return instance;
    }
    protected ImmutableArray<string> GetImmutableColors()
    {
      if (_colorsBuilder == null)
      {
        return _colors;
      }
      else
      {
        return Colors.ToImmutable();
      }
    }
  }
}