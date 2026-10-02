using Metalama.Framework.Aspects;

namespace Metalama.Documentation.QuickStart;

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( $"Executing {meta.Target.Method}." );

        return meta.Proceed();
    }
}