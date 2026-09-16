public class DatabaseFrontend
{
    public int DatabaseCalls { get; private set; }

    [Cache]
    public Entity GetEntity( EntityKey entityKey )
    {
        Console.WriteLine( "Executing GetEntity..." );
        this.DatabaseCalls++;

        return new Entity( entityKey );
    }

    [Cache]
    public string GetInvoiceVersionDetails( InvoiceVersion invoiceVersion )
    {
        Console.WriteLine( "Executing GetInvoiceVersionDetails..." );
        this.DatabaseCalls++;

        return "some details";
    }
}