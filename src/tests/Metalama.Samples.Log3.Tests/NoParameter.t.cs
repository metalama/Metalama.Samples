namespace Metalama.Samples.LogParameters.Tests.NoParameter;
internal class Foo
{
  [Log]
  private void Bar()
  {
    Console.WriteLine($"Foo.Bar() started.");
    try
    {
      object result = null;
      Console.WriteLine($"Foo.Bar() succeeded.");
      return;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Foo.Bar() failed: {e.Message}");
      throw;
    }
  }
}