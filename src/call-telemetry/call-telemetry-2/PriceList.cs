using Metalama.Samples.CallTelemetry.Payments;

namespace Metalama.Samples.CallTelemetry2;

public class PriceList
{
    private readonly PaymentClient _payments = new();

    public decimal GetPriceWithFee( decimal price ) => price + this._payments.GetFee( price );
}
