public class InvoiceVersion : Invoice
{
    [CacheKeyMember]
    public int Version { get; }

    public InvoiceVersion( long id, int version ) : base( id )
    {
        this.Version = version;
    }
}