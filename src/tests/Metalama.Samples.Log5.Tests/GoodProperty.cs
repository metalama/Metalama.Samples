using Microsoft.Extensions.Logging;

namespace Metalama.Samples.Log4.Tests.GoodProperty;

// ReSharper disable once NotAccessedField.Local
internal class Foo
{
    private readonly ILogger _logger;

    public Foo( ILogger logger )
    {
        this._logger = logger;
    }

    [Log]
    public void Method() { }
}