using System.Net.Mail;
using Metalama.Samples.LegacyTesting3;
using Metalama.Samples.LegacyTesting3.Recording;
internal static class MetalamaInterceptors
{
  internal static string BillingDatabase_GetCustomerEmail_Interceptor(int customerId)
  {
    var arguments = new List<object?>();
    arguments.Add(customerId);
    var result = InteractionReplayer.Replay("BillingDatabase.GetCustomerEmail", arguments, typeof(string));
    return (string)result;
  }
  internal static IReadOnlyList<Invoice> BillingDatabase_GetOverdueInvoices_Interceptor(int customerId)
  {
    var arguments = new List<object?>();
    arguments.Add(customerId);
    var result = InteractionReplayer.Replay("BillingDatabase.GetOverdueInvoices", arguments, typeof(IReadOnlyList<Invoice>));
    return (IReadOnlyList<Invoice>)result;
  }
  internal static string File_ReadAllText_Interceptor(string path)
  {
    var arguments = new List<object?>();
    arguments.Add(path);
    var result = InteractionReplayer.Replay("File.ReadAllText", arguments, typeof(string));
    return (string)result;
  }
  internal static void SmtpClient_Send_Interceptor(SmtpClient receiver, string from, string recipients, string? subject, string? body)
  {
    var arguments = new List<object?>();
    arguments.Add(from);
    arguments.Add(recipients);
    arguments.Add(subject);
    arguments.Add(body);
    InteractionReplayer.Replay("SmtpClient.Send", arguments);
  }
  internal static void Thread_Sleep_Interceptor(int millisecondsTimeout)
  {
    var arguments = new List<object?>();
    arguments.Add(millisecondsTimeout);
    InteractionReplayer.Replay("Thread.Sleep", arguments);
  }
}