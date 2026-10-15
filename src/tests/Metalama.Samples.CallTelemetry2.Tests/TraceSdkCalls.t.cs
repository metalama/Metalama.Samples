using Metalama.Samples.CallTelemetry.Payments;
using System.Diagnostics;
namespace Metalama.Samples.CallTelemetry2
{
  // A copy of PaymentsTelemetry.cs. It is in the test file so that the output shows the generated interceptors.
  internal static class PaymentsTelemetry
  {
    public const string Name = "Metalama.Samples.CallTelemetry.Payments";
    public static readonly ActivitySource ActivitySource = new(Name);
    internal static async Task<string> PaymentClient_AuthorizeAsync_Interceptor(PaymentClient receiver, string cardNumber, decimal amount, CancellationToken cancellationToken, string operation)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.AuthorizeAsync");
      activity?.SetTag("code.caller", operation);
      try
      {
        return await receiver.AuthorizeAsync(cardNumber, amount, cancellationToken);
      }
      catch (Exception e)
      {
        activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        throw;
      }
    }
    internal static async Task PaymentClient_CaptureAsync_Interceptor(PaymentClient receiver, string authorizationId, CancellationToken cancellationToken, string operation)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.CaptureAsync");
      activity?.SetTag("code.caller", operation);
      try
      {
        await receiver.CaptureAsync(authorizationId, cancellationToken);
        return;
      }
      catch (Exception e)
      {
        activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        throw;
      }
    }
    internal static decimal PaymentClient_GetFee_Interceptor(PaymentClient receiver, decimal amount, string operation)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.GetFee");
      activity?.SetTag("code.caller", operation);
      try
      {
        return receiver.GetFee(amount);
      }
      catch (Exception e)
      {
        activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        throw;
      }
    }
  }
}
namespace Metalama.Samples.CallTelemetry2.Tests.TraceSdkCalls
{
  public class CheckoutService
  {
    private readonly PaymentClient _payments = new();
    public async Task<string> CheckoutAsync(string cardNumber, decimal amount, CancellationToken cancellationToken = default)
    {
      var fee = PaymentsTelemetry.PaymentClient_GetFee_Interceptor(this._payments, amount: amount, operation: "CheckoutService.CheckoutAsync");
      var authorizationId = await PaymentsTelemetry.PaymentClient_AuthorizeAsync_Interceptor(this._payments, cardNumber: cardNumber, amount: amount + fee, cancellationToken: cancellationToken, operation: "CheckoutService.CheckoutAsync");
      await PaymentsTelemetry.PaymentClient_CaptureAsync_Interceptor(this._payments, authorizationId: authorizationId, cancellationToken: cancellationToken, operation: "CheckoutService.CheckoutAsync");
      return authorizationId;
    }
  }
  public class PriceList
  {
    private readonly PaymentClient _payments = new();
    public decimal GetPriceWithFee(decimal price) => price + PaymentsTelemetry.PaymentClient_GetFee_Interceptor(this._payments, amount: price, operation: "PriceList.GetPriceWithFee");
  }
}