public class PartProvider : IPartProvider
{
    [ReportAndSwallowExceptions]
    public string GetPart( string name ) => throw new Exception( "This method has a bug." );
}