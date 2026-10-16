using Billing;
using Reporting;
using Shipping;

var invoiceService = new InvoiceService();
var shippingService = new ShippingService();
var taxReport = new TaxReport();

foreach ( var region in new[] { "EU", "US", "CH" } )
{
    Console.WriteLine( $"Invoice total in {region}: {invoiceService.ComputeTotal( 100, region ):F2}" );
    Console.WriteLine( $"Shipping cost in {region}: {shippingService.ComputeShippingCost( 2.5m, region ):F2}" );
}

Console.WriteLine( $"Annual tax in EU: {taxReport.ComputeAnnualTax( [1000, 2000, 1500], "EU" ):F2}" );
