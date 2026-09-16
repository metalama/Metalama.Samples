internal class Program
{
    private static void Main()
    {
        Console.WriteLine( "Instantiating an object. Change tracking is disabled by default." );

        var comment =
            new ModeratedComment( Guid.NewGuid(), "Cicero", "Non nobis solum nati sumus" );

        Console.WriteLine( $"IsChanged={comment.IsChanged}" );
        Console.WriteLine( "Start tracking changes." );
        comment.IsTrackingChanges = true;
        Console.WriteLine( $"IsChanged={comment.IsChanged}" );
        Console.WriteLine( "Doing a change." );
        comment.IsApproved = true;
        Console.WriteLine( $"IsChanged={comment.IsChanged}" );
    }
}