using System.ComponentModel.DataAnnotations;
namespace Metalama.Samples.Builder2.Tests.SimpleExample;
#pragma warning disable CS8618 //  Non-nullable property must contain a non-null value when exiting constructor.
[GenerateBuilder]
public partial class Song
{
  [Required]
  public string Artist { get; }
  [Required]
  public string Title { get; }
  public TimeSpan? Duration { get; }
  protected Song(string artist, string title, TimeSpan? duration)
  {
    Artist = artist;
    Title = title;
    Duration = duration;
  }
  public virtual Builder ToBuilder()
  {
    return new Builder(this);
  }
  public class Builder
  {
    public Builder(string artist, string title)
    {
      Artist = artist;
      Title = title;
    }
    protected internal Builder(Song source)
    {
      Artist = source.Artist;
      Title = source.Title;
      Duration = source.Duration;
    }
    public string Artist { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Title { get; set; }
    public Song Build()
    {
      var instance = new Song(Artist, Title, Duration);
      return instance;
    }
  }
}
