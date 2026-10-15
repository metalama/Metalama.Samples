using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Samples.CallTracing.Inventory;
using System.Diagnostics;

namespace Metalama.Samples.CallTracing2;

// Traces the calls to InventoryClient when the TraceCalls MSBuild property is True.
internal class TracingFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // Register the interceptors only when the build sets the TraceCalls property.
        if ( !amender.Project.TryGetProperty( "TraceCalls", out var traceCalls )
             || !bool.TryParse( traceCalls, out var isEnabled ) || !isEnabled )
        {
            return;
        }

        amender.InterceptMethods(
            calls => calls
                .Type( typeof(InventoryClient) )
                .AllMethods()
                .RedirectToSynthesizedMethod( nameof(this.Trace) )
                .WithGranularity( InterceptorGranularity.Member ) );
    }

    [Template]
    public dynamic? Trace()
    {
        var interceptedMethod = meta.Interception.Method.InterceptedMethod;

        // Build the description of the call: the caller, the called method, and the arguments.
        var arguments = new List<string>();

        foreach ( var parameter in meta.Interception.Method.ArgumentParameters )
        {
            arguments.Add( $"{parameter.Name}: {parameter.Value}" );
        }

        var call = $"{meta.Interception.CallingType!.Name}.{meta.Interception.CallingMember!.Name} -> "
                   + $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}"
                   + $"({string.Join( ", ", arguments )})";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = meta.Proceed();

            if ( interceptedMethod.ReturnType.Equals( SpecialType.Void ) )
            {
                Console.WriteLine( $"[trace] {call} succeeded in {stopwatch.Elapsed.TotalMilliseconds:F3} ms." );
            }
            else
            {
                Console.WriteLine(
                    $"[trace] {call} returned {result} in {stopwatch.Elapsed.TotalMilliseconds:F3} ms." );
            }

            return result;
        }
        catch ( Exception e )
        {
            Console.WriteLine(
                $"[trace] {call} threw {e.GetType().Name} in {stopwatch.Elapsed.TotalMilliseconds:F3} ms: {e.Message}" );

            throw;
        }
    }
}
