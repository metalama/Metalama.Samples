[TrackChanges]
[NotifyPropertyChanged]
public partial class Comment
{
    public Guid Id { get; }

    public string Author { get; set; }

    public string Content { get; set; }

    public Comment( Guid id, string author, string content )
    {
        this.Id = id;
        this.Author = author;
        this.Content = content;
    }
}