using Metalama.Samples.CallTelemetry.Payments;
using System.Diagnostics;
using System.Diagnostics.Metrics;
namespace Metalama.Samples.CallTelemetry3
{
  // A copy of PaymentsTelemetry.cs. It is in the test file so that the output shows the generated interceptors.
  internal static class PaymentsTelemetry
  {
    public const string Name = "Metalama.Samples.CallTelemetry.Payments";
    public static readonly ActivitySource ActivitySource = new(Name);
    private static readonly Meter _meter = new(Name);
    public static readonly Counter<long> Calls = _meter.CreateCounter<long>("payments.client.calls");
    public static readonly Histogram<double> Duration = _meter.CreateHistogram<double>("payments.client.duration", unit: "ms");
    internal static async Task<string> PaymentClient_AuthorizeAsync_Interceptor(PaymentClient receiver, string cardNumber, decimal amount, CancellationToken cancellationToken, string operation, string callingType)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.AuthorizeAsync");
      activity?.SetTag("code.caller", operation);
      var startTimestamp = Stopwatch.GetTimestamp();
      try
      {
        return await receiver.AuthorizeAsync(cardNumber, amount, cancellationToken);
      }
      catch (Exception e)
      {
        activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        throw;
      }
      finally
      {
        var tags = new TagList
        {
          {
            "method",
            "PaymentClient.AuthorizeAsync"
          },
          {
            "caller.type",
            callingType
          }
        };
        Calls.Add(1, tags);
        Duration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds, tags);
      }
    }
    internal static async Task PaymentClient_CaptureAsync_Interceptor(PaymentClient receiver, string authorizationId, CancellationToken cancellationToken, string operation, string callingType)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.CaptureAsync");
      activity?.SetTag("code.caller", operation);
      var startTimestamp = Stopwatch.GetTimestamp();
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
      finally
      {
        var tags = new TagList
        {
          {
            "method",
            "PaymentClient.CaptureAsync"
          },
          {
            "caller.type",
            callingType
          }
        };
        Calls.Add(1, tags);
        Duration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds, tags);
      }
    }
    internal static decimal PaymentClient_GetFee_Interceptor(PaymentClient receiver, decimal amount, string operation, string callingType)
    {
      using Activity? activity = ActivitySource.StartActivity("PaymentClient.GetFee");
      activity?.SetTag("code.caller", operation);
      var startTimestamp = Stopwatch.GetTimestamp();
      try
      {
        return receiver.GetFee(amount);
      }
      catch (Exception e)
      {
        activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        throw;
      }
      finally
      {
        var tags = new TagList
        {
          {
            "method",
            "PaymentClient.GetFee"
          },
          {
            "caller.type",
            callingType
          }
        };
        Calls.Add(1, tags);
        Duration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds, tags);
      }
    }
  }
}
namespace Metalama.Samples.CallTelemetry3.Tests.RecordSdkMetrics
{
  public class CheckoutService
  {
    private readonly PaymentClient _payments = new();
    public async Task<string> CheckoutAsync(string cardNumber, decimal amount, CancellationToken cancellationToken = default)
    {
      var fee = PaymentsTelemetry.PaymentClient_GetFee_Interceptor(this._payments, amount: amount, operation: "CheckoutService.CheckoutAsync", callingType: "CheckoutService");
      var authorizationId = await PaymentsTelemetry.PaymentClient_AuthorizeAsync_Interceptor(this._payments, cardNumber: cardNumber, amount: amount + fee, cancellationToken: cancellationToken, operation: "CheckoutService.CheckoutAsync", callingType: "CheckoutService");
      await PaymentsTelemetry.PaymentClient_CaptureAsync_Interceptor(this._payments, authorizationId: authorizationId, cancellationToken: cancellationToken, operation: "CheckoutService.CheckoutAsync", callingType: "CheckoutService");
      return authorizationId;
    }
  }
  public class PriceList
  {
    private readonly PaymentClient _payments = new();
    public decimal GetPriceWithFee(decimal price) => price + PaymentsTelemetry.PaymentClient_GetFee_Interceptor(this._payments, amount: price, operation: "PriceList.GetPriceWithFee", callingType: "PriceList");
  }
}