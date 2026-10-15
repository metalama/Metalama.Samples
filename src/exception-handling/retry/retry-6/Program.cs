using Metalama.Samples.Retry6;

var prices = new PriceService();

Console.WriteLine( $"100 USD = {prices.ConvertPrice( 100, "EUR" )} EUR" );
Console.WriteLine( $"100 USD = {await prices.ConvertPriceAsync( 100, "GBP", CancellationToken.None )} GBP" );
Console.WriteLine( await prices.BuyCurrencyAsync( "EUR", 50, CancellationToken.None ) );
