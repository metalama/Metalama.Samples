using System.ComponentModel.DataAnnotations;
namespace Metalama.Samples.Builder1.Tests.SimpleExample;
[GenerateBuilder]
public partial class Song
{
  [Required]
  public string Artist { get; }
  [Required]
  public string Title { get; }
  public TimeSpan? Duration { get; }
  public string Genre { get; } = "General";
  private Song(string artist, string title, TimeSpan? duration, string genre)
  {
    Artist = artist;
    Title = title;
    Duration = duration;
    Genre = genre;
  }
  public Builder ToBuilder()
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
    internal Builder(Song source)
    {
      Artist = source.Artist;
      Title = source.Title;
      Duration = source.Duration;
      Genre = source.Genre;
    }
    public string Artist { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Genre { get; set; } = "General";
    public string Title { get; set; }
    public Song Build()
    {
      var instance = new Song(Artist, Title, Duration, Genre);
      return instance;
    }
  }
}
