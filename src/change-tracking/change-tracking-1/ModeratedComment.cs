[TrackChanges]
public class ModeratedComment : Comment
{
    public ModeratedComment( Guid id, string author, string content ) : base( id, author, content ) { }

    public bool? IsApproved { get; set; }
}