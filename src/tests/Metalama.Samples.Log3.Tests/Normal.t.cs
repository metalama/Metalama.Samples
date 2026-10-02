namespace Metalama.Samples.LogParameters.Tests.Normal;
internal class Foo
{
  [Log]
  private void Bar(int a, string b)
  {
    Console.WriteLine($"Foo.Bar(a = {{{a}}}, b = {{{b}}}) started.");
    try
    {
      Console.WriteLine($"a={a}, b='{b}'");
      object result = null;
      Console.WriteLine($"Foo.Bar(a = {{{a}}}, b = {{{b}}}) succeeded.");
      return;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Foo.Bar(a = {{{a}}}, b = {{{b}}}) failed: {e.Message}");
      throw;
    }
  }
}