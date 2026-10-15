#if TEST_OPTIONS
// @Include(../../universal-interceptor/weather-app/InterceptionFabric.cs)
#endif

using Metalama.Samples.UniversalInterceptor.Weather;

namespace Metalama.Samples.UniversalInterceptor.Tests.StructReceiver;

// The fabric skips the call on the RequestQuota struct with ForEachSite, so the call is not intercepted, and no warning
// is reported. The call to GetQuota, on a WeatherClient, is intercepted.
public class WeatherReport
{
    private readonly WeatherClient _client = new();

    public bool TrySendRequest()
    {
        var quota = this._client.GetQuota();

        return quota.TryConsume();
    }
}
