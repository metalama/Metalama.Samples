#pragma warning disable CS8618

[TrackChanges]
[NotifyPropertyChanged]
public partial class Comment
{
    public Comment( Guid id, string author, string content )
    {
        this.Id = id;
        this.Author = author;
        this.Content = content;
    }

    public Guid Id { get; }

    public string Author { get; set; }

    public string Content { get; set; }
}