using Metalama.Extensions.Interceptors;
using Metalama.Framework.Fabrics;
using Metalama.Samples.UniversalInterceptor;
using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor1;

// Redirects every call to WeatherClient, in the whole project, to the CallLog.Current interceptor.
internal class InterceptionFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.InterceptMethods(
            calls => calls
                .Type( typeof(WeatherClient) )
                .AllMethods()
                .RedirectToUniversalInterceptor( typeof(CallLog), nameof(CallLog.Current) ) );
}
