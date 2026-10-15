using Metalama.Extensions.Interceptors;
using Metalama.Framework.Fabrics;
using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor2;

// Redirects every call to the types of the weather library, in the whole project, to the CallLog.Current interceptor.
internal class InterceptionFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        foreach ( var type in new[] { typeof(WeatherClient), typeof(RequestQuota) } )
        {
            amender.InterceptMethods(
                calls => calls
                    .Type( type )
                    .AllMethods()
                    .ForEachSite(
                        site =>

                            // The calls on a struct receiver are not intercepted. See the README file.
                            !site.InterceptedMethod.IsStatic && site.Receiver.Type is { IsReferenceType: false }
                                ? site.Skip()
                                : site.RedirectToUniversalInterceptor( typeof(CallLog), nameof(CallLog.Current) ) ) );
        }
    }
}
