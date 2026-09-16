public class Invoice : Entity
{
    public Invoice( long id ) : base( new EntityKey( "Invoice", id ) ) { }
}