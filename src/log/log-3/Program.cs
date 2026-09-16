try
{
    Calculator.Add( 1, 1 );
    Calculator.Divide( 0, 1 );
}
catch ( Exception ex )
{
    Console.WriteLine( ex );
}