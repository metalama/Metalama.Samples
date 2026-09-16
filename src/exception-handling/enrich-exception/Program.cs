internal class Program
{
    private static void Main()
    {
        try
        {
            Calculator.Fibonaci( 5 );
        }
        catch ( Exception e )
        {
            Console.WriteLine( e );
            var context = e.GetContextInfo();

            if ( context != null )
            {
                Console.WriteLine( "---with---" );
                Console.Write( context );
                Console.WriteLine( "----------" );
            }
        }
    }
}