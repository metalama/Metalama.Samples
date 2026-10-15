using System.Net.Mail;
using Metalama.Samples.LegacyTesting2;
using Metalama.Samples.LegacyTesting2.Recording;
internal static class MetalamaInterceptors
{
  internal static string BillingDatabase_GetCustomerEmail_Interceptor(int customerId)
  {
    var arguments = new List<object?>();
    arguments.Add(customerId);
    var result = BillingDatabase.GetCustomerEmail(customerId);
    InteractionRecorder.Record("BillingDatabase.GetCustomerEmail", arguments, result);
    return result;
  }
  internal static IReadOnlyList<Invoice> BillingDatabase_GetOverdueInvoices_Interceptor(int customerId)
  {
    var arguments = new List<object?>();
    arguments.Add(customerId);
    var result = BillingDatabase.GetOverdueInvoices(customerId);
    InteractionRecorder.Record("BillingDatabase.GetOverdueInvoices", arguments, result);
    return result;
  }
  internal static string File_ReadAllText_Interceptor(string path)
  {
    var arguments = new List<object?>();
    arguments.Add(path);
    var result = File.ReadAllText(path);
    InteractionRecorder.Record("File.ReadAllText", arguments, result);
    return result;
  }
  internal static void SmtpClient_Send_Interceptor(SmtpClient receiver, string from, string recipients, string? subject, string? body)
  {
    var arguments = new List<object?>();
    arguments.Add(from);
    arguments.Add(recipients);
    arguments.Add(subject);
    arguments.Add(body);
    receiver.Send(from, recipients, subject, body);
    object result = null;
    InteractionRecorder.Record("SmtpClient.Send", arguments, result);
  }
  internal static void Thread_Sleep_Interceptor(int millisecondsTimeout)
  {
    var arguments = new List<object?>();
    arguments.Add(millisecondsTimeout);
    Thread.Sleep(millisecondsTimeout);
    object result = null;
    InteractionRecorder.Record("Thread.Sleep", arguments, result);
  }
}