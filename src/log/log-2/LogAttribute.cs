using Metalama.Framework.Aspects;

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( $"{meta.Target.Method} started." );

        try
        {
            var result = meta.Proceed();

            Console.WriteLine( $"{meta.Target.Method} succeeded." );

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine( $"{meta.Target.Method} failed: {e.Message}." );

            throw;
        }
    }
}