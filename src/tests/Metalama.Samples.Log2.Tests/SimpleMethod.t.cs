namespace Metalama.Samples.Log.Tests.SimpleMethod;
internal static class Program
{
  [Log]
  public static void Main()
  {
    Console.WriteLine("Program.Main() started.");
    try
    {
      SayHello();
      object result = null;
      Console.WriteLine("Program.Main() succeeded.");
      return;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Program.Main() failed: {e.Message}.");
      throw;
    }
  }
  [Log]
  private static int SayHello()
  {
    Console.WriteLine("Program.SayHello() started.");
    try
    {
      int result;
      Console.WriteLine("Hello, world.");
      result = 5;
      Console.WriteLine("Program.SayHello() succeeded.");
      return result;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Program.SayHello() failed: {e.Message}.");
      throw;
    }
  }
}