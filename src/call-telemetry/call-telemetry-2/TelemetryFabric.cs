using Metalama.Extensions.Interceptors;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Samples.CallTelemetry.Payments;
using System.Diagnostics;

namespace Metalama.Samples.CallTelemetry2;

// Creates a trace span around each call to the payment SDK.
internal class TelemetryFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.InterceptMethods(
            calls => calls
                .Type( typeof(PaymentClient) )
                .AllMethods()
                .RedirectToSynthesizedMethod( nameof(this.Trace) )
                .PlaceInType( typeof(PaymentsTelemetry) )
                .Configure(
                    method =>
                    {
                        // Each call site passes the name of the calling operation, so that the call sites of a
                        // method share the same interceptor.
                        var caller = method.Context.Caller.Member;
                        var operation = method.Method.AddParameter( "operation", typeof(string) );
                        method.SetArgument( operation, (TypedConstant) $"{caller.DeclaringType.Name}.{caller.Name}" );
                    } ) );

    [Template]
    public dynamic? Trace( string operation )
    {
        var interceptedMethod = meta.Interception.Method.InterceptedMethod;

        // The span covers the call. For an async method, meta.Proceed() awaits the call.
        using Activity? activity = meta.ThisType.ActivitySource.StartActivity(
            $"{interceptedMethod.DeclaringType.Name}.{interceptedMethod.Name}" );

        activity?.SetTag( "code.caller", operation );

        try
        {
            return meta.Proceed();
        }
        catch ( Exception e )
        {
            activity?.SetStatus( ActivityStatusCode.Error, e.Message );

            throw;
        }
    }
}
