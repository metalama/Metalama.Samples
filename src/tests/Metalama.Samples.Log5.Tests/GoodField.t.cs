using Microsoft.Extensions.Logging;
namespace Metalama.Samples.Log4.Tests.GoodField;
// ReSharper disable once NotAccessedField.Local
internal class Foo
{
  private readonly ILogger _logger;
  public Foo(ILogger logger)
  {
    this._logger = logger;
  }
  [Log]
  public void Method()
  {
    var logger = _logger;
    var isTracingEnabled = logger.IsEnabled(LogLevel.Trace);
    if (isTracingEnabled)
    {
      logger.LogTrace($"Foo.Method() started.");
    }
    try
    {
      object result = null;
      if (isTracingEnabled)
      {
        logger.LogTrace($"Foo.Method() succeeded.");
      }
      return;
    }
    catch (Exception e)when (logger.IsEnabled(LogLevel.Warning))
    {
      logger.LogWarning($"Foo.Method() failed: {e.Message}");
      throw;
    }
  }
}