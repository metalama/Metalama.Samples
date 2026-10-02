public class DatabaseFrontend
{
    public int DatabaseCalls { get; private set; }

    [Cache]
    public byte[] GetBlob( string container, byte[] hash )
    {
        Console.WriteLine( "Executing GetBlob..." );
        this.DatabaseCalls++;

        return new byte[] { 0, 1, 2 };
    }

    [Cache]
    public byte[] GetBlob( BlobId blobId )
    {
        Console.WriteLine( "Executing GetBlob..." );
        this.DatabaseCalls++;

        return new byte[] { 0, 1, 2 };
    }
}