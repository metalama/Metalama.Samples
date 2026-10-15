using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Samples.CallTelemetry.Payments;
using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry3;

// Creates a trace span around each call to the payment SDK, and records the number and the duration of the calls.
internal class TelemetryFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.InterceptMethods(
            calls => calls
                .Type( typeof(PaymentClient) )
                .AllMethods()
                .RedirectToSynthesizedMethod( nameof(this.Monitor) )
                .PlaceInType( typeof(PaymentsTelemetry) )
                .Configure(
                    method =>
                    {
                        // Each call site passes the name of the calling operation and of the calling type, so that
                        // the call sites of a method share the same interceptor.
                        var caller = method.Context.Caller.Member;

                        var operation = method.Method.AddParameter( "operation", typeof(string) );
                        method.SetArgument( operation, (TypedConstant) $"{caller.DeclaringType.Name}.{caller.Name}" );

                        var callingType = method.Method.AddParameter( "callingType", typeof(string) );
                        method.SetArgument( callingType, (TypedConstant) caller.DeclaringType.Name );
                    } ) );

    [Template]
    public dynamic? Monitor( string operation, string callingType )
    {
        var interceptedMethod = meta.Interception.Method.InterceptedMethod;
        var methodName = $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}";

        // The span covers the call. For an async method, meta.Proceed() awaits the call.
        using Activity? activity = meta.ThisType.ActivitySource.StartActivity( methodName );

        activity?.SetTag( "code.caller", operation );

        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            return meta.Proceed();
        }
        catch ( Exception e )
        {
            activity?.SetStatus( ActivityStatusCode.Error, e.Message );

            throw;
        }
        finally
        {
            // The metrics are tagged with the calling type, not with the calling operation, to keep the number of
            // distinct tag values small.
            var tags = new TagList { { "method", methodName }, { "caller.type", callingType } };

            meta.ThisType.Calls.Add( 1, tags );
            meta.ThisType.Duration.Record( Stopwatch.GetElapsedTime( startTimestamp ).TotalMilliseconds, tags );
        }
    }
}
