#pragma warning disable CS0169, CS8618, IDE0044, IDE0051

namespace Metalama.Samples.Log4.Tests.FieldOrWrongType;

internal class Foo
{
    private TextWriter _logger;

    [Log]
    public void Bar() { }
}